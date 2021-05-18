#!/usr/bin/env python
"""Convert text to upper case"""

import inkex

class Uppercase(inkex.TextExtension):
    """To upper case"""
    def process_chardata(self, text):
        return text.upper()

if __name__ == '__main__':
    Uppercase().run()
