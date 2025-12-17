// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_cache.model.dart';

// **************************************************************************
// TypeAdapterGenerator
// **************************************************************************

class AppCacheAdapter extends TypeAdapter<AppCache> {
  @override
  final int typeId = 2;

  @override
  AppCache read(BinaryReader reader) {
    final numOfFields = reader.readByte();
    final fields = <int, dynamic>{
      for (int i = 0; i < numOfFields; i++) reader.readByte(): reader.read(),
    };
    return AppCache()
      ..key = fields[0] as String
      ..value = fields[1] as String
      ..createdAt = fields[2] as DateTime
      ..lastAccessed = fields[3] as DateTime
      ..expiresAt = fields[4] as DateTime?
      ..category = fields[5] as String?
      ..priority = fields[6] as int?
      ..isActive = fields[7] as bool
      ..description = fields[8] as String?;
  }

  @override
  void write(BinaryWriter writer, AppCache obj) {
    writer
      ..writeByte(9)
      ..writeByte(0)
      ..write(obj.key)
      ..writeByte(1)
      ..write(obj.value)
      ..writeByte(2)
      ..write(obj.createdAt)
      ..writeByte(3)
      ..write(obj.lastAccessed)
      ..writeByte(4)
      ..write(obj.expiresAt)
      ..writeByte(5)
      ..write(obj.category)
      ..writeByte(6)
      ..write(obj.priority)
      ..writeByte(7)
      ..write(obj.isActive)
      ..writeByte(8)
      ..write(obj.description);
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AppCacheAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}

class AppSettingsAdapter extends TypeAdapter<AppSettings> {
  @override
  final int typeId = 3;

  @override
  AppSettings read(BinaryReader reader) {
    final numOfFields = reader.readByte();
    final fields = <int, dynamic>{
      for (int i = 0; i < numOfFields; i++) reader.readByte(): reader.read(),
    };
    return AppSettings()
      ..isFirstTimeCheck = fields[0] as bool
      ..isSystemInitialized = fields[1] as bool
      ..lastRoute = fields[2] as String?
      ..theme = fields[3] as String?
      ..language = fields[4] as String?
      ..fontSize = fields[5] as double?
      ..apiTimeout = fields[6] as int?
      ..baseUrl = fields[7] as String?
      ..enableLogging = fields[8] as bool?
      ..createdAt = fields[9] as DateTime
      ..updatedAt = fields[10] as DateTime
      ..isActive = fields[11] as bool;
  }

  @override
  void write(BinaryWriter writer, AppSettings obj) {
    writer
      ..writeByte(12)
      ..writeByte(0)
      ..write(obj.isFirstTimeCheck)
      ..writeByte(1)
      ..write(obj.isSystemInitialized)
      ..writeByte(2)
      ..write(obj.lastRoute)
      ..writeByte(3)
      ..write(obj.theme)
      ..writeByte(4)
      ..write(obj.language)
      ..writeByte(5)
      ..write(obj.fontSize)
      ..writeByte(6)
      ..write(obj.apiTimeout)
      ..writeByte(7)
      ..write(obj.baseUrl)
      ..writeByte(8)
      ..write(obj.enableLogging)
      ..writeByte(9)
      ..write(obj.createdAt)
      ..writeByte(10)
      ..write(obj.updatedAt)
      ..writeByte(11)
      ..write(obj.isActive);
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AppSettingsAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}
