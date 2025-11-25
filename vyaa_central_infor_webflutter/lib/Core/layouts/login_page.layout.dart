import 'package:flutter/material.dart';

class LoginPageLayout extends StatelessWidget {
  final Widget leftColumn;
  final Widget rightColumn;

  const LoginPageLayout({
    Key? key,
    required this.leftColumn,
    required this.rightColumn,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        // Primera columna
        Expanded(
          flex: 65,
          child: leftColumn,
        ),
        // Segunda columna
        Expanded(
          flex: 35,
          child: rightColumn,
        ),
      ],
    );
  }
}