#!/usr/bin/env python
"""Remove colors"""

import inkex

class Desaturate(inkex.ColorExtension):
    """Remove color but maintain intesity"""
    def modify_color(self, name, color):
        lum = (max(color.red, color.green, color.blue) \
             + min(color.red, color.green, color.blue)) // 2
        return inkex.Color((int(round(lum)), int(round(lum)), int(round(lum))))

if __name__ == '__main__':
    Desaturate().run()
