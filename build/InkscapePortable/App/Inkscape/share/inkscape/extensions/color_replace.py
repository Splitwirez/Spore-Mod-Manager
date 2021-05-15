#!/usr/bin/env python
"""Replace color extension"""

import inkex

class ReplaceColor(inkex.ColorExtension):
    """Replace color in SVG with another"""
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument('-f', "--from_color",\
            default=inkex.Color("black"), type=inkex.Color, help="Replace color")
        pars.add_argument('-t', "--to_color",\
            default=inkex.Color("red"), type=inkex.Color, help="By color")

    def modify_color(self, name, color):
        return self.options.to_color if color == self.options.from_color else color

if __name__ == '__main__':
    ReplaceColor().run()
