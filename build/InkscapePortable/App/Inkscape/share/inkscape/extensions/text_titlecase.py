#!/usr/bin/env python
"""Convert to title case"""

import inkex

class TitleCase(inkex.TextExtension):
    """To titlecase"""
    word_ended = True

    def process_chardata(self, text):
        ret = ""
        newline = True
        for char in text:
            if char.isspace() or newline:
                self.word_ended = True
            if not char.isspace():
                newline = False

            if self.word_ended and char.isalpha():
                ret += char.upper()
                self.word_ended = False
            elif char.isalpha():
                ret += char.lower()
            else:
                ret += char

        return ret

if __name__ == '__main__':
    TitleCase().run()
