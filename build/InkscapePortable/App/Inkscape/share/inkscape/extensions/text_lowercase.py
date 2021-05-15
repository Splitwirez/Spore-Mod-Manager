#!/usr/bin/env python
"""Make text lower case"""

import inkex

class Lowercase(inkex.TextExtension):
    """Convert to lowercase"""
    def process_chardata(self, text):
        return text.lower()

if __name__ == '__main__':
    Lowercase().run()
