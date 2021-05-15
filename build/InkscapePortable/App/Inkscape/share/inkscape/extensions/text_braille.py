#!/usr/bin/env python
"""Convert the text to braille text"""

import inkex

# https://en.wikipedia.org/wiki/Braille_ASCII#Braille_ASCII_values
U2801_MAP = "A1B'K2L@CIF/MSP\"E3H9O6R^DJG>NTQ,*5<-U8V.%[$+X!&;:4\\0Z7(_?W]#Y)="

class Braille(inkex.TextExtension):
    """Convert to ASCII Braille"""
    @staticmethod
    def map_char(char):
        """Map a single letter to braille"""
        assert isinstance(char, str)
        try:
            mapint = U2801_MAP.index(char.upper())
        except ValueError:
            return char
        return chr(mapint + 0x2801)

if __name__ == '__main__':
    Braille().run()
