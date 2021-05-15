#!/usr/bin/env python
"""Increase satuation"""

import inkex

class MoreSaturation(inkex.ColorExtension):
    """Increase saturation of selected objects"""
    def modify_color(self, name, color):
        color.saturation += int(0.05 * 255.0)
        return color

if __name__ == '__main__':
    MoreSaturation().run()
