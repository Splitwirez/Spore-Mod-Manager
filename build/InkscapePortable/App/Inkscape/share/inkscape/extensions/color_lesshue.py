#!/usr/bin/env python
"""Reduce hue"""

import inkex

class LessHue(inkex.ColorExtension):
    """Remove Hue from the color"""
    def modify_color(self, name, color):
        color.hue -= int(0.05 * 255)
        return color

if __name__ == '__main__':
    LessHue().run()
