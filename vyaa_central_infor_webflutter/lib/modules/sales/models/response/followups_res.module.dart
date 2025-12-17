class FollowupRes {
  final String id;
  final DateTime date;
  final String comment;
  final String userId;
  final DateTime createdAt;

  FollowupRes({
    required this.id,
    required this.date,
    required this.comment,
    required this.userId,
    required this.createdAt,
  });

  factory FollowupRes.fromJson(Map<String, dynamic> json) {
    return FollowupRes(
      id: (json['id'] ?? 0).toString(), // Convertir int a String
      date: DateTime.parse(json['date']),
      comment: json['comment'] ?? '',
      userId: (json['userId'] ?? 0).toString(), // Convertir int a String por si acaso
      createdAt: DateTime.parse(json['createdAt']),
    );
  }
  /*
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'date': date.toIso8601String(),
      'comment': comment,
      'userId': userId,
      'createdAt': createdAt.toIso8601String(),
    };
  }
  */
}
