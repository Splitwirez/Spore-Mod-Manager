#!/usr/bin/env python
"""Increase lightness"""

import inkex

class MoreLight(inkex.ColorExtension):
    """Lighten selected objects"""
    def modify_color(self, name, color):
        color.lightness += int(0.05 * 255.0)
        return color

if __name__ == '__main__':
    MoreLight().run()
