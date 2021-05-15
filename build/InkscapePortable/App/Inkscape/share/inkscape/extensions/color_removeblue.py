#!/usr/bin/env python
"""Extension to remove the blue colour from selected shapes"""

import inkex

class RemoveBlue(inkex.ColorExtension):
    """Remove blue color from selected objects"""
    def modify_color(self, name, color):
        return inkex.Color([color.red, color.green, 0])

if __name__ == '__main__':
    RemoveBlue().run()
