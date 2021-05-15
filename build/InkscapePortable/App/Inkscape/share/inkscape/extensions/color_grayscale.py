#!/usr/bin/env python
"""Convert to grey"""

import inkex

class Grayscale(inkex.ColorExtension):
    """Make all colours grayscale"""
    def modify_color(self, name, color):
        # ITU-R Recommendation BT.709 (NTSC and PAL)
        # l = 0.2125 * r + 0.7154 * g + 0.0721 * b
        lum = 0.299 * color.red + 0.587 * color.green + 0.114 * color.blue
        return inkex.Color((int(round(lum)), int(round(lum)), int(round(lum))))

if __name__ == '__main__':
    Grayscale().run()
