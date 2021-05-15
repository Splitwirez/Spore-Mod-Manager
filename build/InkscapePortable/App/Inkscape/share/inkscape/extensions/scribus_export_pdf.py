#!/bin/env python3
# coding=utf-8
#
# Copyright (C) 2019 Marc Jeanmougin, Cédric Gémy, a-l-e
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA
#


import os
import re
import sys
import inkex
from inkex import AbortExtension
from inkex.base import TempDirMixin
from inkex.command import take_snapshot, call


SCRIBUS_EXE = "scribus"
VERSION_REGEX = re.compile(r"(\d+)\.(\d+)\.(\d+)")


# several things could be taken into consideration here :
# - the fact that openDoc works on svg files is a workaround
# - the commented parts should be the correct way to do things
#   and even include a possibility to add margins
#   BUT currently fails to place the SVG document
#   (object placed top-left instead of SVG placed top-left)
class Scribus(TempDirMixin, inkex.OutputExtension):
    def add_arguments(self, arg_parser):
        arg_parser.add_argument("--pdf-version", type=int, dest="pdfVersion", default="13",
                                help="PDF version (see Scribus documentation)")
        arg_parser.add_argument("--bleed", type=float, dest="bleed", default="0",
                                help="Bleed value")
        arg_parser.add_argument("--bleed-marks", type=inkex.Boolean, dest="bleedMarks",
                default=False, help="Bleed marks")
        arg_parser.add_argument("--color-marks", type=inkex.Boolean, dest="colorMarks",
                default=False, help="Color Marks")
        arg_parser.add_argument("--intent", type=int, dest="intent", default="0",
                                help="0: Perceptual, 1: Relative Colorimetric, 2: Saturation, 3: Absolute Colorimetric")
        arg_parser.add_argument("--title", type=str, dest="title", default="", help="PDF title")
        #arg_parser.add_argument("--fonts", type=int, dest="fonts", default="1",
        #                        help="Embed fonts : 0 for embedding, 1 to convert to path, 2 to prevent embedding")

    def generate_script(self, stream, width, height, icc):
        margin = self.options.bleed
        pdfVersion = self.options.pdfVersion
        embedFonts = 1 #self.options.fonts
        bleedMarks = self.options.bleedMarks
        colorMarks = self.options.colorMarks
        if ((bleedMarks or colorMarks) and margin < 7):
            raise AbortExtension("You need 7mm bleed to show cutting marks or color marks")
        if (bleedMarks or colorMarks):
            margin = margin - 7 #because scribus is weird. At the time of 1.5.5, it adds 7 when those are set.
        stream.write("""
import scribus
import sys
icc = "{icc}"
margin = {margin}
class exportPDF():
    def __init__(self, svg=sys.argv[1], o=sys.argv[2]):
        #scribus.newDocument(({width},{height}), (margin,margin,margin,margin),
        #                    PORTRAIT, 1, UNIT_MILLIMETERS, PAGE_1, 0, 1)
        #scribus.placeSVG(svg, 0, 0)
        scribus.openDoc(svg)
        pdf = scribus.PDFfile()
        scribus.setUnit(UNIT_MILLIMETERS)
        pdf.bleedl = margin
        pdf.bleedr = margin
        pdf.bleedt = margin
        pdf.bleedb = margin
        pdf.useDocBleeds = False
        pdf.cropMarks = {bleedMarks}
        pdf.bleedMarks = {bleedMarks}
        pdf.colorMarks = {colorMarks}
        pdf.version = {pdfVersion}
        pdf.allowAnnots = True
        pdf.allowChange = True
        pdf.allowCopy = True
        pdf.allowPrinting = True
        pdf.noembicc = False #embed icc !
        pdf.solidpr = icc
        pdf.imagepr = icc
        pdf.printprofc = icc
        pdf.intenti = {self.options.intent}
        pdf.intents = {self.options.intent}
        pdf.info = "{self.options.title}"
        pdf.profiles = True
        pdf.profilei = True
        pdf.outdst = 1 # output destination : 0=screen, 1=printer
        pdf.file = o
        pdf.compress = True
        pdf.compressmtd = 0 # 0 = automatic, 1 = jpeg ; 2 = zip, 3 = none
        pdf.quality = 0 #max
        pdf.fontEmbedding = {embedFonts}
        pdf.thumbnails = True

        pdf.save()
exportPDF()""".format(**locals()))

    def save(self, stream):
        scribus_version = call(SCRIBUS_EXE, '-g', '--version').decode('utf-8')
        version_match = VERSION_REGEX.search(scribus_version)
        if version_match is None:
            raise AbortExtension("Could not detect Scribus version ({})".format(scribus_version))
        major = int(version_match.group(1))
        minor = int(version_match.group(2))
        point = int(version_match.group(3))
        if (major < 1) or (major == 1 and minor < 5):
            raise AbortExtension("Found Scribus {}. This extension requires Scribus 1.5.x".format(version_match.group(0)))

        input_file = self.options.input_file
        py_file = os.path.join(self.tempdir, 'scribus.py')
        svg_file = os.path.join(self.tempdir, 'in.svg')
        profiles = self.svg.defs.findall("svg:color-profile")
        if len(profiles) == 0:
            raise AbortExtension("You did not link a color profile in this document.")
        elif len(profiles) > 1:
            raise AbortExtension("More than one color profiles are linked in this document. No output generated.")
        iccPath = profiles[0].get("xlink:href")


        with open(input_file) as f:
            with open(svg_file, "w") as f1:
                for line in f:
                    f1.write(line)
            f.close()

        pdf_file = os.path.join(self.tempdir, 'out.pdf')
        width = self.svg.unittouu(self.svg.get('width'))
        height = self.svg.unittouu(self.svg.get('height'))

        with open(py_file, 'w') as fhl:
            self.generate_script(fhl, width, height, iccPath)
        call(SCRIBUS_EXE, '-g', '-py', py_file, svg_file, pdf_file)
        with open(pdf_file, 'rb') as fhl:
            stream.write(fhl.read())

if __name__ == '__main__':
    Scribus().run()

