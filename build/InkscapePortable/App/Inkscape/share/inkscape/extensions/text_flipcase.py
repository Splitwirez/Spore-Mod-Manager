#!/usr/bin/env python
"""Flip upper to lower and lower to upper cases"""

import inkex

class FlipCase(inkex.TextExtension):
    """Change the case, cHANGE THE CASE"""
    @staticmethod
    def map_char(char):
        return char.upper() if char.islower() else char.lower()

if __name__ == '__main__':
    FlipCase().run()
