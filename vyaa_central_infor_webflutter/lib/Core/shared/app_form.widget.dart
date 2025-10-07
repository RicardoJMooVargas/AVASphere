import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

/// Tipos de campos disponibles para el formulario
enum FormFieldType {
  text,
  email,
  password,
  number,
  date,
  datetime,
  dropdown,
  checkbox,
  textarea,
  phone,
}

/// Configuración de un campo del formulario
class FormFieldConfig {
  final String label;
  final String? hint;
  final FormFieldType type;
  final bool isRequired;
  final TextEditingController? controller;
  final List<DropdownItem>? dropdownItems;
  final bool? checkboxValue;
  final ValueChanged<bool?>? onCheckboxChanged;
  final ValueChanged<dynamic>? onDropdownChanged;
  final ValueChanged<DateTime?>? onDateChanged;
  final String? Function(dynamic)? validator;
  final TextInputType? keyboardType;
  final List<TextInputFormatter>? inputFormatters;
  final int? maxLines;
  final int? maxLength;
  final Widget? prefixIcon;
  final Widget? suffixIcon;
  final bool enabled;
  final DateTime? initialDate;
  final DateTime? firstDate;
  final DateTime? lastDate;

  FormFieldConfig({
    required this.label,
    this.hint,
    this.type = FormFieldType.text,
    this.isRequired = false,
    this.controller,
    this.dropdownItems,
    this.checkboxValue,
    this.onCheckboxChanged,
    this.onDropdownChanged,
    this.onDateChanged,
    this.validator,
    this.keyboardType,
    this.inputFormatters,
    this.maxLines,
    this.maxLength,
    this.prefixIcon,
    this.suffixIcon,
    this.enabled = true,
    this.initialDate,
    this.firstDate,
    this.lastDate,
  });
}

/// Elemento de dropdown
class DropdownItem {
  final String label;
  final dynamic value;

  DropdownItem({
    required this.label,
    required this.value,
  });
}

/// Sección del formulario para agrupar campos
class FormSection {
  final String title;
  final List<FormFieldConfig> fields;
  final Widget? headerWidget;
  final bool isCollapsible;
  final bool initiallyExpanded;

  FormSection({
    required this.title,
    required this.fields,
    this.headerWidget,
    this.isCollapsible = false,
    this.initiallyExpanded = true,
  });
}

/// Widget de formulario dinámico y reutilizable
class AppForm extends StatefulWidget {
  final List<FormSection> sections;
  final GlobalKey<FormState>? formKey;
  final EdgeInsets? padding;
  final double spacing;
  final double sectionSpacing;
  final Widget? header;
  final Widget? footer;
  final CrossAxisAlignment crossAxisAlignment;
  final bool shrinkWrap;
  final ScrollPhysics? physics;

  const AppForm({
    Key? key,
    required this.sections,
    this.formKey,
    this.padding,
    this.spacing = 16.0,
    this.sectionSpacing = 24.0,
    this.header,
    this.footer,
    this.crossAxisAlignment = CrossAxisAlignment.start,
    this.shrinkWrap = false,
    this.physics,
  }) : super(key: key);

  @override
  State<AppForm> createState() => _AppFormState();
}

class _AppFormState extends State<AppForm> {
  late GlobalKey<FormState> _formKey;
  final Map<String, bool> _sectionExpansionState = {};

  @override
  void initState() {
    super.initState();
    _formKey = widget.formKey ?? GlobalKey<FormState>();
    
    // Inicializar estado de expansión de secciones
    for (final section in widget.sections) {
      if (section.isCollapsible) {
        _sectionExpansionState[section.title] = section.initiallyExpanded;
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: _formKey,
      child: SingleChildScrollView(
        physics: widget.physics,
        padding: widget.padding ?? const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: widget.crossAxisAlignment,
          children: [
            if (widget.header != null) ...[
              widget.header!,
              SizedBox(height: widget.sectionSpacing),
            ],
            
            ...widget.sections.map((section) => _buildSection(section)).toList(),
            
            if (widget.footer != null) ...[
              SizedBox(height: widget.sectionSpacing),
              widget.footer!,
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildSection(FormSection section) {
    final sectionContent = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (section.headerWidget != null) ...[
          section.headerWidget!,
          SizedBox(height: widget.spacing),
        ],
        
        ...section.fields.map((field) => Padding(
          padding: EdgeInsets.only(bottom: widget.spacing),
          child: _buildFormField(field),
        )).toList(),
      ],
    );

    if (section.isCollapsible) {
      return Padding(
        padding: EdgeInsets.only(bottom: widget.sectionSpacing),
        child: Card(
          child: ExpansionTile(
            title: Text(
              section.title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            initiallyExpanded: _sectionExpansionState[section.title] ?? true,
            onExpansionChanged: (expanded) {
              setState(() {
                _sectionExpansionState[section.title] = expanded;
              });
            },
            children: [
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: sectionContent,
              ),
            ],
          ),
        ),
      );
    }

    return Padding(
      padding: EdgeInsets.only(bottom: widget.sectionSpacing),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (section.title.isNotEmpty) ...[
            Text(
              section.title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            SizedBox(height: widget.spacing),
          ],
          sectionContent,
        ],
      ),
    );
  }

  Widget _buildFormField(FormFieldConfig config) {
    switch (config.type) {
      case FormFieldType.checkbox:
        return _buildCheckboxField(config);
      case FormFieldType.dropdown:
        return _buildDropdownField(config);
      case FormFieldType.date:
      case FormFieldType.datetime:
        return _buildDateField(config);
      case FormFieldType.textarea:
        return _buildTextAreaField(config);
      default:
        return _buildTextField(config);
    }
  }

  Widget _buildTextField(FormFieldConfig config) {
    return TextFormField(
      controller: config.controller,
      decoration: InputDecoration(
        labelText: config.isRequired ? '${config.label} *' : config.label,
        hintText: config.hint,
        prefixIcon: config.prefixIcon,
        suffixIcon: config.suffixIcon,
        border: const OutlineInputBorder(),
      ),
      keyboardType: config.keyboardType ?? _getKeyboardType(config.type),
      inputFormatters: config.inputFormatters,
      maxLength: config.maxLength,
      obscureText: config.type == FormFieldType.password,
      enabled: config.enabled,
      validator: config.validator ?? (config.isRequired ? (value) {
        if (value == null || value.trim().isEmpty) {
          return '${config.label} es requerido';
        }
        return null;
      } : null),
    );
  }

  Widget _buildTextAreaField(FormFieldConfig config) {
    return TextFormField(
      controller: config.controller,
      decoration: InputDecoration(
        labelText: config.isRequired ? '${config.label} *' : config.label,
        hintText: config.hint,
        prefixIcon: config.prefixIcon,
        suffixIcon: config.suffixIcon,
        border: const OutlineInputBorder(),
        alignLabelWithHint: true,
      ),
      keyboardType: TextInputType.multiline,
      maxLines: config.maxLines ?? 3,
      maxLength: config.maxLength,
      enabled: config.enabled,
      validator: config.validator ?? (config.isRequired ? (value) {
        if (value == null || value.trim().isEmpty) {
          return '${config.label} es requerido';
        }
        return null;
      } : null),
    );
  }

  Widget _buildCheckboxField(FormFieldConfig config) {
    return Row(
      children: [
        Checkbox(
          value: config.checkboxValue ?? false,
          onChanged: config.enabled ? config.onCheckboxChanged : null,
        ),
        Expanded(
          child: Text(
            config.isRequired ? '${config.label} *' : config.label,
            style: Theme.of(context).textTheme.bodyMedium,
          ),
        ),
      ],
    );
  }

  Widget _buildDropdownField(FormFieldConfig config) {
    return DropdownButtonFormField<dynamic>(
      decoration: InputDecoration(
        labelText: config.isRequired ? '${config.label} *' : config.label,
        hintText: config.hint,
        prefixIcon: config.prefixIcon,
        suffixIcon: config.suffixIcon,
        border: const OutlineInputBorder(),
      ),
      items: config.dropdownItems?.map((item) => DropdownMenuItem<dynamic>(
        value: item.value,
        child: Text(item.label),
      )).toList(),
      onChanged: config.enabled ? config.onDropdownChanged : null,
      validator: config.validator ?? (config.isRequired ? (value) {
        if (value == null) {
          return '${config.label} es requerido';
        }
        return null;
      } : null),
    );
  }

  Widget _buildDateField(FormFieldConfig config) {
    return TextFormField(
      controller: config.controller,
      decoration: InputDecoration(
        labelText: config.isRequired ? '${config.label} *' : config.label,
        hintText: config.hint,
        prefixIcon: config.prefixIcon ?? const Icon(Icons.calendar_today),
        suffixIcon: config.suffixIcon,
        border: const OutlineInputBorder(),
      ),
      readOnly: true,
      enabled: config.enabled,
      onTap: config.enabled ? () => _selectDate(config) : null,
      validator: config.validator ?? (config.isRequired ? (value) {
        if (value == null || value.trim().isEmpty) {
          return '${config.label} es requerido';
        }
        return null;
      } : null),
    );
  }

  Future<void> _selectDate(FormFieldConfig config) async {
    final DateTime now = DateTime.now();
    final DateTime initialDate = config.initialDate ?? now;
    final DateTime firstDate = config.firstDate ?? DateTime(now.year - 10);
    final DateTime lastDate = config.lastDate ?? DateTime(now.year + 10);

    if (config.type == FormFieldType.datetime) {
      final DateTime? date = await showDatePicker(
        context: context,
        initialDate: initialDate,
        firstDate: firstDate,
        lastDate: lastDate,
      );

      if (date != null) {
        final TimeOfDay? time = await showTimePicker(
          context: context,
          initialTime: TimeOfDay.fromDateTime(initialDate),
        );

        if (time != null) {
          final DateTime dateTime = DateTime(
            date.year,
            date.month,
            date.day,
            time.hour,
            time.minute,
          );
          
          config.controller?.text = _formatDateTime(dateTime);
          config.onDateChanged?.call(dateTime);
        }
      }
    } else {
      final DateTime? date = await showDatePicker(
        context: context,
        initialDate: initialDate,
        firstDate: firstDate,
        lastDate: lastDate,
      );

      if (date != null) {
        config.controller?.text = _formatDate(date);
        config.onDateChanged?.call(date);
      }
    }
  }

  TextInputType _getKeyboardType(FormFieldType type) {
    switch (type) {
      case FormFieldType.email:
        return TextInputType.emailAddress;
      case FormFieldType.number:
        return TextInputType.number;
      case FormFieldType.phone:
        return TextInputType.phone;
      case FormFieldType.textarea:
        return TextInputType.multiline;
      default:
        return TextInputType.text;
    }
  }

  String _formatDate(DateTime date) {
    return '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
  }

  String _formatDateTime(DateTime dateTime) {
    return '${_formatDate(dateTime)} ${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';
  }

  /// Método público para validar el formulario
  bool validate() {
    return _formKey.currentState?.validate() ?? false;
  }

  /// Método público para guardar el formulario
  void save() {
    _formKey.currentState?.save();
  }
}