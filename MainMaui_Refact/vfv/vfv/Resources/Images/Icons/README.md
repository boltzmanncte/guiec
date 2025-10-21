# Lucide Icons for MAUI

This folder contains SVG icons exported from the Lucide icon library used in FigmaWebApp.

## Icon List

### Toolbar Icons
- **folder_open.svg** - Open File button
- **play.svg** - Start/Play execution button
- **square.svg** - Stop execution button
- **wrench.svg** - Tools menu button
- **settings.svg** - Settings button
- **help_circle.svg** - Help menu button

### File Management Icons
- **chevron_up.svg** - Move file up
- **chevron_down.svg** - Move file down
- **trash_2.svg** - Delete files
- **file_text.svg** - File list item icon

### UI Icons
- **alert_circle.svg** - Error/alert indicator
- **alert_triangle.svg** - Warning indicator (available)
- **x.svg** - Close/dismiss button

## Icon Properties
- **Format**: SVG (Scalable Vector Graphics)
- **Viewbox**: 24x24
- **Stroke Width**: 1.5px (elegant, thin strokes)
- **Stroke Style**: Round caps and joins
- **Color**: White (configured via TintColor in .csproj)

## Usage in XAML

Icons are automatically converted to PNG by MAUI build system and can be referenced in XAML:

```xml
<!-- As button icon -->
<Button Text="Open File"
        ImageSource="folder_open.png" />

<!-- As image -->
<Image Source="file_text.png"
       WidthRequest="20"
       HeightRequest="20" />
```

## Source
Icons based on [Lucide Icons](https://lucide.dev/) - An open-source icon library with over 1,000+ icons.
