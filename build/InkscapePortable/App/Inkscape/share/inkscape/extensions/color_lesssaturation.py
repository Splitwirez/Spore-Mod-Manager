#!/usr/bin/env python
"""Reduce saturation"""

import inkex

class LessSaturation(inkex.ColorExtension):
    """Make colours less saturated"""
    def modify_color(self, name, color):
        color.saturation -= int(0.05 * 255)
        return color

if __name__ == '__main__':
    LessSaturation().run()
