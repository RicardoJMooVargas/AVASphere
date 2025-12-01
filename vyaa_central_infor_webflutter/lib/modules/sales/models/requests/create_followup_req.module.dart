import 'package:flutter/widgets.dart';

class CreateFollowupReq {
  final TextEditingController commentController;
  final DateTime date;

  CreateFollowupReq({String? comment, DateTime? date})
    : commentController = TextEditingController(text: comment ?? ''),
      date = date ?? DateTime.now();

  /// Create from existing controllers
  CreateFollowupReq.fromControllers({
    required this.commentController,
    required this.date,
  });

  /// Produce a Map ready for JSON encoding
  Map<String, dynamic> toJson() {
    return {
      'date': date.toIso8601String(),
      'comment': commentController.text.trim(),
    };
  }

  /// Convenience factory from raw data
  factory CreateFollowupReq.fromRaw({String? comment, DateTime? date}) =>
      CreateFollowupReq(comment: comment, date: date);

  /// Dispose all controllers
  void dispose() {
    commentController.dispose();
  }
}
