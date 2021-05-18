#!/usr/bin/env python
"""Darken colours of selected objects"""

import inkex

class Darker(inkex.ColorExtension):
    """Make the colours darker"""
    def modify_color(self, name, color):
        factor = 0.9
        if color.space == 'hsl':
            color.lightness = int(round(max(color.lightness * factor, 0)))
        else:
            color.red = int(round(max(color.red * factor, 0)))
            color.green = int(round(max(color.green * factor, 0)))
            color.blue = int(round(max(color.blue * factor, 0)))
        return color

if __name__ == '__main__':
    Darker().run()
