
enum HttpMethod { get, post, put, delete, patch }
enum SystemModule {
  general(0, 'General'),
  common(1, 'Common'),
  system(2, 'System'),
  sales(3, 'Sales'),
  projects(4, 'Projects'),
  inventory(5, 'Inventory'),
  shopping(6, 'Shopping');

  const SystemModule(this.value, this.displayName);
  
  final int value;
  final String displayName;
}