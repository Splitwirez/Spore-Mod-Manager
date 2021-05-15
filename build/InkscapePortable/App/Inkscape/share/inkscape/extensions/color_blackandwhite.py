#!/usr/bin/env python
"""To black and white"""

import inkex

class BlackAndWhite(inkex.ColorExtension):
    """Convert colours to black and white"""
    def add_arguments(self, pars):
        pars.add_argument("-t", "--threshold", type=int, default=127, help="Threshold Color Level")

    def modify_color(self, name, color):
        # ITU-R Recommendation BT.709 (NTSC and PAL)
        # l = 0.2125 * r + 0.7154 * g + 0.0721 * b
        lum = 0.299 * color.red + 0.587 * color.green + 0.114 * color.blue
        grey = 255 if lum > self.options.threshold else 0
        return inkex.Color((grey, grey, grey))

if __name__ == '__main__':
    BlackAndWhite().run()
