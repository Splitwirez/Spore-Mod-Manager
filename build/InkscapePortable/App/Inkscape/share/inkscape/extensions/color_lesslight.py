#!/usr/bin/env python
"""Reduce lightness"""

import inkex

class LessLight(inkex.ColorExtension):
    """Reduce the light of the color"""
    def modify_color(self, name, color):
        color.lightness -= int(0.05 * 255)
        return color

if __name__ == '__main__':
    LessLight().run()
