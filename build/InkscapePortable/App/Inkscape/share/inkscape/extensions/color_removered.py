#!/usr/bin/env python
"""Extension for removing the colour red from selected objects"""

import inkex

class RemoveRed(inkex.ColorExtension):
    """Remove red color from selected objects"""
    def modify_color(self, name, color):
        return inkex.Color([0, color.green, color.blue])

if __name__ == '__main__':
    RemoveRed().run()
