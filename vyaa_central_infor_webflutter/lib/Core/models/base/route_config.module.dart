import 'package:flutter/material.dart';
import 'package:get/get.dart';

// Layouts imports
import 'package:vyaa_central_infor_webflutter/Core/layouts/sidebar_layout.dart';
import 'package:vyaa_central_infor_webflutter/Core/layouts/main_app_layout.dart';
import 'package:vyaa_central_infor_webflutter/Core/Widgets/system/app_sidebar.widget.dart';
// Enums
import 'package:vyaa_central_infor_webflutter/configs/enums.dart';

class RouteConfig {
  final String name;
  final String path;
  final Widget Function() page;
  final ScreenTypeCore screenType;
  final LayoutType layoutType;
  final List<GetMiddleware> middlewares;
  final bool requiresAuth;
  final bool isFullScreen;
  final bool isSubScreen;
  final String? title;
  final String? description;
  final IconData? icon;
  final Color? backgroundColor;
  final bool showInSidebar;
  final int? sidebarOrder;
  final List<String>? requiredPermissions;
  final Duration? transitionDuration;
  final Transition? transition;

  const RouteConfig({
    required this.name,
    required this.path,
    required this.page,
    this.screenType = ScreenTypeCore.app,
    this.layoutType = LayoutType.sidebar,
    this.middlewares = const [],
    this.requiresAuth = true,
    this.isFullScreen = false,
    this.isSubScreen = false,
    this.title,
    this.description,
    this.icon,
    this.backgroundColor,
    this.showInSidebar = false,
    this.sidebarOrder,
    this.requiredPermissions,
    this.transitionDuration,
    this.transition,
  });
}