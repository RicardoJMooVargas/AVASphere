// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user_session.model.dart';

// **************************************************************************
// TypeAdapterGenerator
// **************************************************************************

class UserSessionAdapter extends TypeAdapter<UserSession> {
  @override
  final int typeId = 1;

  @override
  UserSession read(BinaryReader reader) {
    final numOfFields = reader.readByte();
    final fields = <int, dynamic>{
      for (int i = 0; i < numOfFields; i++) reader.readByte(): reader.read(),
    };
    return UserSession()
      ..userId = fields[0] as String
      ..username = fields[1] as String?
      ..email = fields[2] as String?
      ..token = fields[3] as String?
      ..refreshToken = fields[4] as String?
      ..loginTime = fields[5] as DateTime
      ..lastActivity = fields[6] as DateTime?
      ..expiresAt = fields[7] as DateTime?
      ..isActive = fields[8] as bool
      ..rememberMe = fields[9] as bool
      ..permissions = fields[10] as String?
      ..roles = fields[11] as String?
      ..deviceInfo = fields[12] as String?
      ..ipAddress = fields[13] as String?;
  }

  @override
  void write(BinaryWriter writer, UserSession obj) {
    writer
      ..writeByte(14)
      ..writeByte(0)
      ..write(obj.userId)
      ..writeByte(1)
      ..write(obj.username)
      ..writeByte(2)
      ..write(obj.email)
      ..writeByte(3)
      ..write(obj.token)
      ..writeByte(4)
      ..write(obj.refreshToken)
      ..writeByte(5)
      ..write(obj.loginTime)
      ..writeByte(6)
      ..write(obj.lastActivity)
      ..writeByte(7)
      ..write(obj.expiresAt)
      ..writeByte(8)
      ..write(obj.isActive)
      ..writeByte(9)
      ..write(obj.rememberMe)
      ..writeByte(10)
      ..write(obj.permissions)
      ..writeByte(11)
      ..write(obj.roles)
      ..writeByte(12)
      ..write(obj.deviceInfo)
      ..writeByte(13)
      ..write(obj.ipAddress);
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UserSessionAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}
