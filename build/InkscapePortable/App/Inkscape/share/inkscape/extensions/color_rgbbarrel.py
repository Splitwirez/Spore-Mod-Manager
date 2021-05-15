#!/usr/bin/env python
"""Rotate the colors in the selected elements"""

import inkex

class RgbBarrel(inkex.ColorExtension):
    """
    Cycle colors RGB -> BRG

    aka  Do a Barrel Roll!
    """
    def modify_color(self, name, color):
        return inkex.Color((color.blue, color.red, color.green))

if __name__ == '__main__':
    RgbBarrel().run()
