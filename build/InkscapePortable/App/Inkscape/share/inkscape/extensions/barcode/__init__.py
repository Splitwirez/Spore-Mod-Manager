# coding=utf-8
#
# Copyright (C) 2014 Martin Owens
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#
# pylint: disable=no-self-use
"""
Renderer for barcodes, SVG extension for Inkscape.

For supported barcodes see Barcode module directory.
"""


# This lists all known Barcodes missing from this package
# ===== UPC-Based Extensions ====== #
# Code11
# ========= Code25-Based ========== #
# Codabar
# Postnet
# ITF25
# ========= Alpha-numeric ========= #
# Code39Mod
# USPS128
# =========== 2D Based ============ #
# PDF417
# PDF417-Macro
# PDF417-Truncated
# PDF417-GLI

class NoBarcode(object):
    """Simple class for no barcode"""

    def __init__(self, msg):
        self.msg = msg

    def encode(self, text):
        """Encode the text into a barcode pattern"""
        raise ValueError("No barcode encoder: {}".format(self.msg))

    def generate(self):
        """Generate actual svg from the barcode pattern"""
        return None


def get_barcode(code, **kw):
    """Gets a barcode from a list of available barcode formats"""
    if not code:
        return NoBarcode("No barcode format given.")

    code = str(code).replace('-', '').strip()
    module = 'barcode.' + code
    lst = ['barcode']
    try:
        return getattr(__import__(module, fromlist=lst), code)(kw)
    except ImportError as err:
        if code in str(err):
            return NoBarcode("Invalid type of barcode: {}.{}".format(module, code))
        raise
    except AttributeError:
        return NoBarcode("Barcode module is missing barcode class: {}.{}".format(module, code))
