#!/usr/bin/env python
"""Remove green color from selected objects"""

import inkex

class RemoveGreen(inkex.ColorExtension):
    """Remove green color from selected objects"""
    def modify_color(self, name, color):
        return inkex.Color([color.red, 0, color.blue])

if __name__ == '__main__':
    RemoveGreen().run()
