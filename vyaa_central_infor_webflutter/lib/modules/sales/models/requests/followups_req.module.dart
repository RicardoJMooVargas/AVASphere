import 'package:flutter/widgets.dart';

class FollowupReq {
  final TextEditingController commentController;
  final TextEditingController userIdController;
  final DateTime date;

  FollowupReq({
    String? comment,
    String? userId,
    DateTime? date,
  })  : commentController = TextEditingController(text: comment ?? ''),
        userIdController = TextEditingController(text: userId ?? ''),
        date = date ?? DateTime.now();

  /// Create from existing controllers
  FollowupReq.fromControllers({
    required this.commentController,
    required this.userIdController,
    required this.date,
  });

  /// Produce a Map ready for JSON encoding (solo campos necesarios)
  Map<String, dynamic> toJson() {
    return {
      'date': date.toIso8601String(),
      'comment': commentController.text.trim(),
      'userId': userIdController.text.trim(),
    };
  }

  /// Convenience factory from raw data
  factory FollowupReq.fromRaw({
    String? comment,
    String? userId,
    DateTime? date,
  }) =>
      FollowupReq(
        comment: comment,
        userId: userId,
        date: date,
      );

  /// Dispose all controllers
  void dispose() {
    commentController.dispose();
    userIdController.dispose();
  }
}