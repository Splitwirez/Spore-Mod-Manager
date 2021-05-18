#!/usr/bin/env python
"""Lighten colours of selected objects"""

import inkex

class Brighter(inkex.ColorExtension):
    """Make colours brighter"""
    def modify_color(self, name, color):
        factor = 0.9
        contra = int(1 / (1 - factor))
        if color.space == 'hsl':
            color.lightness = min(int(round(color.lightness / factor)), 255)
        elif color.red == 0 and color.green == 0 and color.blue == 0:
            color.red = contra
            color.green = contra
            color.blue = contra
        else:
            color.red = min(int(round(color.red / factor)), 255)
            color.green = min(int(round(color.green / factor)), 255)
            color.blue = min(int(round(color.blue / factor)), 255)
        return color

if __name__ == '__main__':
    Brighter().run()
