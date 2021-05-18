#!/usr/bin/env python
"""Adjust all the HSL values"""

import random
import inkex

class HslAdjust(inkex.ColorExtension):
    """Modify the HSL levels of each color"""
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("-x", "--hue", type=int, default=0, help="Adjust hue")
        pars.add_argument("-s", "--saturation", type=int, default=0, help="Adjust saturation")
        pars.add_argument("-l", "--lightness", type=int, default=0, help="Adjust lightness")
        pars.add_argument("--random_h", type=inkex.Boolean, dest="random_hue")
        pars.add_argument("--random_s", type=inkex.Boolean, dest="random_saturation")
        pars.add_argument("--random_l", type=inkex.Boolean, dest="random_lightness")

    def modify_color(self, name, color):
        if self.options.random_hue:
            color.hue = int(random.random() * 255.0)
        elif self.options.hue:
            color.hue += (self.options.hue * 2.55)

        if self.options.random_saturation:
            color.saturation = int(random.random() * 255.0)
        elif self.options.saturation:
            color.saturation += (self.options.saturation * 2.55)

        if self.options.random_lightness:
            color.lightness = int(random.random() * 255.0)
        elif self.options.lightness:
            color.lightness += (self.options.lightness * 2.55)

        return color

if __name__ == '__main__':
    HslAdjust().run()
