// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'system_config.model.dart';

// **************************************************************************
// TypeAdapterGenerator
// **************************************************************************

class SystemConfigAdapter extends TypeAdapter<SystemConfig> {
  @override
  final int typeId = 0;

  @override
  SystemConfig read(BinaryReader reader) {
    final numOfFields = reader.readByte();
    final fields = <int, dynamic>{
      for (int i = 0; i < numOfFields; i++) reader.readByte(): reader.read(),
    };
    return SystemConfig()
      ..hasConfiguration = fields[0] as bool
      ..requiresMigration = fields[1] as bool
      ..message = fields[2] as String
      ..details = fields[3] as String?
      ..lastUpdated = fields[4] as DateTime
      ..isActive = fields[5] as bool
      ..serverVersion = fields[6] as String?
      ..environment = fields[7] as String?
      ..rawJsonData = fields[8] as String?;
  }

  @override
  void write(BinaryWriter writer, SystemConfig obj) {
    writer
      ..writeByte(9)
      ..writeByte(0)
      ..write(obj.hasConfiguration)
      ..writeByte(1)
      ..write(obj.requiresMigration)
      ..writeByte(2)
      ..write(obj.message)
      ..writeByte(3)
      ..write(obj.details)
      ..writeByte(4)
      ..write(obj.lastUpdated)
      ..writeByte(5)
      ..write(obj.isActive)
      ..writeByte(6)
      ..write(obj.serverVersion)
      ..writeByte(7)
      ..write(obj.environment)
      ..writeByte(8)
      ..write(obj.rawJsonData);
  }

  @override
  int get hashCode => typeId.hashCode;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is SystemConfigAdapter &&
          runtimeType == other.runtimeType &&
          typeId == other.typeId;
}
