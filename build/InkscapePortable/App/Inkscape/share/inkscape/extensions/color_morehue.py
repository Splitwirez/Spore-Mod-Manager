#!/usr/bin/env python
"""Add more hue"""

import inkex

class MoreHue(inkex.ColorExtension):
    """Add hue to any selected object"""
    def modify_color(self, name, color):
        color.hue += int(0.05 * 255.0)
        return color

if __name__ == '__main__':
    MoreHue().run()
