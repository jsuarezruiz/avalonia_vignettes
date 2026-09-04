// Run with `flutter test /absolute/path/to/this/file.dart` from the original dog_slider
// Flutter project. See README.md beside this file for provenance and reproduction.
import 'dart:convert';
import 'dart:io';
import 'dart:math' as math;
import 'dart:typed_data';
import 'dart:ui' as ui;
import 'package:flutter_test/flutter_test.dart';
import 'package:flare_flutter/flare.dart';
import 'package:flare_flutter/base/actor_color.dart';
import 'package:flare_flutter/base/actor_path.dart';
import 'package:flare_flutter/base/actor_shape.dart';
import 'package:flare_flutter/base/path_point.dart';

const fps = 60;
const output = String.fromEnvironment('DOG_OUTPUT');

void main() {
  test('export original dog vector poses', () async {
    expect(output.isNotEmpty, isTrue, reason: 'Pass --dart-define=DOG_OUTPUT=/absolute/output/path');
    final bytes = await File('lib/assets/DogAnimation.flr').readAsBytes();
    final actor = await FlutterActor.loadFromByteData(ByteData.sublistView(bytes));
    final source = actor.getArtboard()!;
    final animations = <String, Object>{};
    for (final name in ['walk', 'sit-front']) {
      final board = source.makeInstance() as FlutterActorArtboard;
      board.initializeGraphics();
      final animation = board.getAnimation(name)!;
      final count = (animation.duration * fps).round();
      final frames = <Object>[];
      for (var frame = 0; frame < count + (animation.isLooping ? 0 : 1); frame++) {
        animation.apply(math.min(frame / fps, animation.duration), board, 1);
        board.advance(0);
        frames.add(board.drawableNodes.whereType<FlutterActorShape>()
            .where((s) => s.doesDraw).map(exportShape).toList());
      }
      animations[name] = {'duration': animation.duration, 'loop': animation.isLooping, 'frames': frames};
      // A reference rendered by the original runtime, not by the Avalonia port.
      final recorder = ui.PictureRecorder();
      final canvas = ui.Canvas(recorder)..translate(0, 100)..scale(100 / board.width)
        ..translate(0, -board.height);
      board.draw(canvas);
      final image = await recorder.endRecording().toImage(100, 100);
      final png = await image.toByteData(format: ui.ImageByteFormat.png);
      await File('$output.$name.png').writeAsBytes(png!.buffer.asUint8List());
      image.dispose();
      print('$name: ${frames.length} vector poses, ${animation.duration}s');
    }
    await File(output).writeAsBytes(gzip.encode(utf8.encode(jsonEncode({
      'width': source.width, 'height': source.height, 'fps': fps, 'animations': animations,
    }))));
  });
}

Map<String, Object> exportShape(FlutterActorShape shape) {
  // Fail explicitly if an updated source needs a feature this exporter does not handle.
  expect(shape.transformAffectsStroke, isFalse);
  expect(shape.blendMode, ui.BlendMode.srcOver);
  final clips = <Object>[];
  for (final group in shape.clipShapes) {
    for (final clip in group) {
      if (clip.shape.renderCollapsed) continue;
      expect(clip.intersect, isTrue);
      clips.add({'path': shapePath(clip.shape), 'evenOdd': clip.shape.fill?.fillRule == FillRule.evenOdd});
    }
  }
  return {
    'path': shapePath(shape),
    'evenOdd': shape.fill?.fillRule == FillRule.evenOdd,
    'clips': clips,
    'fills': shape.fills.map((fill) {
      expect(fill, isA<FlutterColorFill>());
      return color((fill as FlutterColorFill).uiColor);
    }).toList(),
    'strokes': shape.strokes.map((stroke) {
      expect(stroke, isA<FlutterColorStroke>());
      expect(stroke.isTrimmed, isFalse);
      return {'color': color((stroke as FlutterColorStroke).uiColor),
        'width': stroke.width, 'cap': stroke.cap.name, 'join': stroke.join.name};
    }).toList(),
  };
}

String color(ui.Color value) => '#${value.toARGB32().toRadixString(16).padLeft(8, '0')}';
String number(double value) => value.toStringAsFixed(5);

String shapePath(ActorShape shape) => shape.paths.map(pathData).join(' ');

String pathData(ActorBasePath path) {
  final geometry = path as FlutterPathPointsPath;
  final points = roundedPoints(geometry.deformedPoints, geometry.isClosed);
  if (points.isEmpty) return '';
  String point(Vec2D p) {
    final world = Vec2D.transformMat2D(Vec2D(), p, path.pathTransform);
    return '${number(world[0])},${number(world[1])}';
  }
  final result = StringBuffer('M${point(points.first.translation)}');
  final segments = geometry.isClosed ? points.length : points.length - 1;
  for (var i = 0; i < segments; i++) {
    final a = points[i];
    final b = points[(i + 1) % points.length];
    if (a is CubicPathPoint || b is CubicPathPoint) {
      result.write(' C${point(a is CubicPathPoint ? a.outPoint : a.translation)} '
          '${point(b is CubicPathPoint ? b.inPoint : b.translation)} ${point(b.translation)}');
    } else {
      result.write(' L${point(b.translation)}');
    }
  }
  if (geometry.isClosed) result.write(' Z');
  return result.toString();
}

// Flare's rounded-corner construction, preserving its 0.55 arc constant.
// Derived from flare_flutter 3.0.2 (Copyright 2018 2D, Inc., MIT; see LICENSE.flare).
List<PathPoint> roundedPoints(List<PathPoint> points, bool closed) {
  final result = <PathPoint>[];
  PathPoint? previous = closed ? points.last : null;
  for (var i = 0; i < points.length; i++) {
    final p = points[i];
    if (p is! StraightPathPoint || p.radius <= 0 || (!closed && (i == 0 || i == points.length - 1))) {
      result.add(p);
      previous = p;
      continue;
    }
    final next = points[(i + 1) % points.length];
    final toPrevious = Vec2D.subtract(Vec2D(), previous is CubicPathPoint ? previous.outPoint : previous!.translation, p.translation);
    final toNext = Vec2D.subtract(Vec2D(), next is CubicPathPoint ? next.inPoint : next.translation, p.translation);
    final previousLength = Vec2D.length(toPrevious);
    final nextLength = Vec2D.length(toNext);
    Vec2D.scale(toPrevious, toPrevious, 1 / previousLength);
    Vec2D.scale(toNext, toNext, 1 / nextLength);
    final radius = math.min(previousLength, math.min(nextLength, p.radius));
    final entry = Vec2D.scaleAndAdd(Vec2D(), p.translation, toPrevious, radius);
    result.add(CubicPathPoint.fromValues(entry, entry,
        Vec2D.scaleAndAdd(Vec2D(), p.translation, toPrevious, .45 * radius)));
    final exit = Vec2D.scaleAndAdd(Vec2D(), p.translation, toNext, radius);
    previous = CubicPathPoint.fromValues(exit,
        Vec2D.scaleAndAdd(Vec2D(), p.translation, toNext, .45 * radius), exit);
    result.add(previous);
  }
  return result;
}
