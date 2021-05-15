#!/usr/bin/env python
# coding=utf-8
#  nicechart.py
#
#  Copyright 2011-2016
#
#  Christoph Sterz
#  Florian Weber
#  Maren Hachmann
#
#  This program is free software; you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation; either version 3 of the License, or
#  (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program; if not, write to the Free Software
#  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
#  MA 02110-1301, USA.
#
# pylint: disable=attribute-defined-outside-init

# TODO / Ideas:
# allow negative values for bar charts
# show values for stacked bar charts
# don't create a new layer for each chart, but a normal group
# correct bar height for stacked bars (it's only half as high as it should be, double)
# adjust position of heading
# use aliasing workaround for stacked bars (e.g. let the rectangles overlap)

# The extension creates one chart for a single value column in one go,
# e.g. chart all temperatures for all months of the year 1978 into one chart.
# (for this, select column 0 for labels and column 1 for values).
# "1978" etc. can be used as heading (Need not be numeric. If not used delete the heading line.)
# Month names can be used as labels
# Values can be shown, in addition to labels (doesn't work with stacked bar charts)
# Values can contain commas as decimal separator, as long as delimiter isn't comma
# Negative values are not yet supported.

# See tests/data/nicechart_01.csv for example data

import re
import csv
import math

from argparse import ArgumentTypeError

import inkex
from inkex.utils import filename_arg
from inkex import Filter, TextElement, Circle, Rectangle
from inkex.paths import Move, line

# www.sapdesignguild.org/goodies/diagram_guidelines/color_palettes.html#mss
COLOUR_TABLE = {
    "red": ["#460101", "#980101", "#d40000", "#f44800", "#fb8b00", "#eec73e", "#d9bb7a", "#fdd99b"],
    "blue": ["#000442", "#0F1781", "#252FB7", "#3A45E1", "#656DDE", "#8A91EC"],
    "gray": ["#222222", "#444444", "#666666", "#888888", "#aaaaaa", "#cccccc", "#eeeeee"],
    "contrast": ["#0000FF", "#FF0000", "#00FF00", "#CF9100", "#FF00FF", "#00FFFF"],
    "sap": ["#f8d753", "#5c9746", "#3e75a7", "#7a653e", "#e1662a", "#74796f", "#c4384f",
            "#fff8a3", "#a9cc8f", "#b2c8d9", "#bea37a", "#f3aa79", "#b5b5a9", "#e6a5a5"]
}

class NiceChart(inkex.GenerateExtension):
    """
    Inkscape extension that can draw pie charts and bar charts
    (stacked, single, horizontally or vertically)
    with optional drop shadow, from a csv file or from pasted text
    """
    container_layer = True

    @property
    def container_label(self):
        """Layer title/label"""
        return 'Chart-Layer: {}'.format(self.options.what)

    def add_arguments(self, pars):
        pars.add_argument('--tab')
        pars.add_argument('--encoding', default='utf-8')
        pars.add_argument('-w', '--what', default='22,11,67', help='Chart Values')
        pars.add_argument("-t", "--type", type=self.arg_method('render'),
                          default=self.render_bar, help="Chart Type")
        pars.add_argument("-b", "--blur", type=inkex.Boolean, default=True, help="Blur Type")
        pars.add_argument("-f", "--filename", type=filename_arg, help="Name of File")
        pars.add_argument("-i", "--input_type", default='file', help="Chart Type")
        pars.add_argument("-d", "--delimiter", default=';', help="delimiter")
        pars.add_argument("-c", "--colors", default='default', help="color-scheme")
        pars.add_argument("--colors_override", help="color-scheme-override")
        pars.add_argument("--reverse_colors", type=inkex.Boolean, default=False,
                          help="reverse color-scheme")
        pars.add_argument("-k", "--col_key", type=int, default=0,
                          help="column that contains the keys")
        pars.add_argument("-v", "--col_val", type=int, default=1,
                          help="column that contains the values")
        pars.add_argument("--headings", type=inkex.Boolean, default=True,
                          help="first line of the CSV file consists of headings for the columns")
        pars.add_argument("-r", "--rotate", type=inkex.Boolean, default=False,
                          help="Draw barchart horizontally")
        pars.add_argument("-W", "--bar-width", type=int, default=10, help="width of bars")
        pars.add_argument("-p", "--pie-radius", type=int, default=100, help="radius of pie-charts")
        pars.add_argument("-H", "--bar-height", type=int, default=100, help="height of bars")
        pars.add_argument("-O", "--bar-offset", type=int, default=5, help="distance between bars")
        pars.add_argument("--stroke-width", type=float, default=1.0)
        pars.add_argument("-o", "--text-offset", type=int, default=5,
                          help="distance between bar and descriptions")
        pars.add_argument("--heading-offset", type=int, default=50,
                          help="distance between chart and chart title")
        pars.add_argument("--segment-overlap", type=inkex.Boolean, default=False,
                          help="Remove aliasing effects by letting pie chart segments overlap")
        pars.add_argument("-F", "--font", default='sans-serif', help="font of description")
        pars.add_argument("-S", "--font-size", type=int, default=10,
                          help="font size of description")
        pars.add_argument("-C", "--font-color", default='black', help="font color of description")

        pars.add_argument("-V", "--show_values", type=inkex.Boolean, default=False,
                          help="Show values in chart")

    def get_data(self):
        """Process the data"""
        col_key = self.options.col_key
        col_val = self.options.col_val

        def process_value(val):
            """Confirm the values from files or direct"""
            val = float(val)
            if val < 0:
                raise inkex.AbortExtension("Negative values are currently not supported!")
            return val

        if self.options.input_type == "file":
            if self.options.filename is None:
                raise inkex.AbortExtension("Filename not specified!")

            # Future: use encoding when opening the file here (if ever needed)
            with open(self.options.filename, "r") as fhl:
                reader = csv.reader(fhl, delimiter=self.options.delimiter)
                title = col_val

                if self.options.headings:
                    header = next(reader)
                    title = header[col_val]

                values = [(line[col_key], process_value(line[col_val])) for line in reader]
                return (title,) + tuple(zip(*values))

        elif self.options.input_type == "direct_input":
            (keys, values) = zip(*[l.split(':', 1) for l in self.options.what.split(',')])
            return ('Direct Input', keys, [process_value(val) for val in values])

        raise inkex.AbortExtension("Unknown input type")

    def get_blur(self):
        """Add blur to the svg and return if needed"""
        if self.options.blur:
            defs = self.svg.defs
            # Create new Filter
            filt = defs.add(Filter(height='3', width='3', x='-0.5', y='-0.5'))
            # Append Gaussian Blur to that Filter
            filt.add_primitive('feGaussianBlur', stdDeviation='1.1')
            return 'filter:url(#%s);' % filt.get_id()
        return ''

    def get_color(self):
        """Get the next available color"""
        if not hasattr(self, '_colors'):
            # Generate list of available colours
            if self.options.colors_override:
                colors = self.options.colors_override.strip()
            else:
                colors = self.options.colors

            if colors[0].isalpha():
                colors = COLOUR_TABLE.get(colors.lower(), COLOUR_TABLE['red'])

            else:
                colors = re.findall("(#[0-9a-fA-F]{6})", colors)
                # to be sure we create a fallback:
                if not colors:
                    colors = COLOUR_TABLE['red']

            if self.options.reverse_colors:
                colors.reverse()
            # Cache the list of colours for later use
            self._colors = colors
            self._color_index = 0

        color = self._colors[self._color_index]
        # Increase index to the next available color
        self._color_index = (self._color_index + 1) % len(self._colors)
        return color

    def generate(self):
        """Generates a nice looking chart into SVG document."""

        # Process the data from a file or text box
        (self.title, keys, values) = self.get_data()
        if not values:
            raise inkex.AbortExtension("No data to render into a chart.")

        # Get the page attributes:
        self.width = self.svg.unittouu(self.svg.get('width'))
        self.height = self.svg.unittouu(self.svg.attrib['height'])
        self.fontoff = float(self.options.font_size) / 3

        # Check if a drop shadow should be drawn:
        self.blur = self.get_blur()

        # Draw the right type of chart
        for elem in self.options.type(keys, values):
            yield elem

    def draw_header(self, heading_x):
        """Draw an optional header text"""
        if self.options.headings and self.title:
            headingtext = self.draw_text(self.title, 4, anchor='end')
            headingtext.set("y", str(self.height / 2 + self.options.heading_offset))
            headingtext.set("x", str(heading_x))
            return headingtext
        return None

    def render_bar(self, keys, values):
        """Draw bar chart"""
        bar_height = self.options.bar_height
        bar_width = self.options.bar_width
        bar_offset = self.options.bar_offset

        # Normalize the bars to the largest value
        value_max = max(list(values) + [0.0])

        # Draw Single bars with their shadows
        for cnt, value in enumerate(values):
            # Draw each bar a set amount offset
            offset = cnt * (bar_width + bar_offset)
            bar_value = (value / value_max) * bar_height

            # Calculate the location of the bar
            x = self.width / 2 + offset
            y = self.height / 2 - int(bar_value)
            width = bar_width
            height = int(bar_value)

            if self.options.rotate:
                # Rotate the bar and align to the left
                x, y, width, height = y, x, height, width
                x += width

            for elem in self.draw_rectangle(x, y, width, height):
                yield elem

            # If keys are given, create text elements
            if keys:
                text = self.draw_text(keys[cnt], anchor='end')
                if not self.options.rotate:  # =vertical
                    text.set("transform", "rotate(-90)")
                    # y after rotation:
                    text.set("x", "-" + str(self.height / 2 + self.options.text_offset))
                    # x after rotation:
                    text.set("y", str(self.width / 2 + offset + bar_width / 2 + self.fontoff))
                else:  # =horizontal
                    text.set("y", str(self.width / 2 + offset + bar_width / 2 + self.fontoff))
                    text.set("x", str(self.height / 2 - self.options.text_offset))

                yield text

            if self.options.show_values:
                vtext = self.draw_text(int(value))
                if not self.options.rotate:  # =vertical
                    vtext.set("transform", "rotate(-90)")
                    # y after rotation:
                    vtext.set("x", "-" + str(self.height / 2 + value - self.options.text_offset))
                    # x after rotation:
                    vtext.set("y", str(self.width / 2 + offset + bar_width / 2 + self.fontoff))
                else:  # =horizontal
                    vtext.set("y", str(self.width / 2 + offset + bar_width / 2 + self.fontoff))
                    vtext.set("x", str(self.height / 2 + value + self.options.text_offset))
                yield vtext

        yield self.draw_header(self.width / 2)

    def draw_rectangle(self, x, y, width, height):
        """Draw a rectangle bar with optional shadow"""
        if self.blur:
            shadow = Rectangle(x=str(x+1), y=str(y+1), width=str(width), height=str(height))
            shadow.set("style", self.blur)
            yield shadow

        rect = Rectangle(x=str(x), y=str(y), width=str(width), height=str(height))
        rect.set("style", "fill:" + self.get_color())
        yield rect

    def draw_text(self, text, add_size=0, anchor='start', **kwargs):
        """Draw a textual label"""
        vtext = TextElement(**kwargs)
        vtext.style = {
            'fill': self.options.font_color,
            'font-family': self.options.font,
            'font-size': str(self.options.font_size + add_size) + 'px',
            'font-style': 'normal',
            'font-variant': 'normal',
            'font-weight': 'normal',
            'font-stretch': 'normal',
            '-inkscape-font-specification': 'Bitstream Charter',
            'text-align': anchor,
            'text-anchor': anchor,
        }
        vtext.text = str(text)
        return vtext

    def render_pie_abs(self, keys, values):
        """Draw a pie chart, with absolute values"""
        # pie_abs = True
        for elem in self.render_pie(keys, values, True):
            yield elem

    def render_pie(self, keys, values, pie_abs=False):
        """Draw pie chart"""
        pie_radius = self.options.pie_radius

        # Iterate all values to draw the different slices
        color = 0
        x = float(self.width) / 2
        y = float(self.height) / 2

        # Create the shadow first (if it should be created):
        if self.blur:
            shadow = Circle(cx=str(x), cy=str(y))
            shadow.set('r', str(pie_radius))
            shadow.set("style", self.blur + "fill:#000000")
            yield shadow

        # Add a grey background circle with a light stroke
        background = Circle(cx=str(x), cy=str(y))
        background.set("r", str(pie_radius))
        background.set("style", "stroke:#ececec;fill:#f9f9f9")
        yield background

        # create value sum in order to divide the slices
        try:
            valuesum = sum(values)
        except ValueError:
            valuesum = 0

        if pie_abs:
            valuesum = 100

        # Set an offsetangle
        offset = 0

        # Draw single slices
        for cnt, value in enumerate(values):
            # Calculate the PI-angles for start and end
            angle = (2 * 3.141592) / valuesum * float(value)
            start = offset
            end = offset + angle

            # proper overlapping
            if self.options.segment_overlap:
                if cnt != len(values) - 1:
                    end += 0.09  # add a 5° overlap
                if cnt == 0:
                    start -= 0.09  # let the first element overlap into the other direction

            # then add the slice
            pieslice = inkex.PathElement()
            pieslice.set('sodipodi:type', 'arc')
            pieslice.set('sodipodi:cx', x)
            pieslice.set('sodipodi:cy', y)
            pieslice.set('sodipodi:rx', pie_radius)
            pieslice.set('sodipodi:ry', pie_radius)
            pieslice.set('sodipodi:start', start)
            pieslice.set('sodipodi:end', end)
            pieslice.set("style", "fill:" + self.get_color() + ";stroke:none;fill-opacity:1")
            ang = angle / 2 + offset

            # If text is given, draw short paths and add the text
            if keys:
                elem = inkex.PathElement()
                elem.path = [
                    Move(
                        (self.width / 2) + pie_radius * math.cos(ang),
                        (self.height / 2) + pie_radius * math.sin(ang),
                    ), line(
                        (self.options.text_offset - 2) * math.cos(ang),
                        (self.options.text_offset - 2) * math.sin(ang),
                    ),
                ]

                elem.style = {
                    'fill': 'none',
                    'stroke': self.options.font_color,
                    'stroke-width': self.options.stroke_width,
                    'stroke-linecap': 'butt',
                }
                yield elem

                label = keys[cnt]
                if self.options.show_values:
                    label += ' ({}{})'.format(str(value), ('', '%')[pie_abs])

                # check if it is right or left of the Pie
                anchor = 'start' if math.cos(ang) > 0 else 'end'
                text = self.draw_text(label, anchor=anchor)

                off = pie_radius + self.options.text_offset
                text.set("x", (self.width / 2) + off * math.cos(ang))
                text.set("y", (self.height / 2) + off * math.sin(ang) + self.fontoff)
                yield text

            # increase the rotation-offset and the colorcycle-position
            offset = offset + angle
            color = (color + 1) % 8

            # append the objects to the extension-layer
            yield pieslice

        yield self.draw_header(self.width / 2 - pie_radius)

    def render_stbar(self, keys, values):
        """Draw stacked bar chart"""

        # Iterate over all values to draw the different slices
        color = 0

        # create value sum in order to divide the bars
        try:
            valuesum = sum(values)
        except ValueError:
            valuesum = 0.0

        # Init offset
        offset = 0
        width = self.options.bar_width
        height = self.options.bar_height
        x = self.width / 2
        y = self.height / 2

        if self.blur:
            if self.options.rotate:
                width, height = height, width
                shy = y
            else:
                shy = str(y - self.options.bar_height)

            # Create rectangle element
            shadow = Rectangle(
                x=str(x), y=str(shy),
                width=str(width), height=str(height),
            )

            # Set shadow blur (connect to filter object in xml path)
            shadow.set("style", self.blur)
            yield shadow

        # Draw Single bars
        for cnt, value in enumerate(values):

            # Calculate the individual heights normalized on 100units
            normedvalue = (self.options.bar_height / valuesum) * float(value)

            # Create rectangle element
            rect = Rectangle()

            # Set chart position to center of document.
            if not self.options.rotate:
                rect.set('x', str(self.width / 2))
                rect.set('y', str(self.height / 2 - offset - normedvalue))
                rect.set("width", str(self.options.bar_width))
                rect.set("height", str(normedvalue))
            else:
                rect.set('x', str(self.width / 2 + offset))
                rect.set('y', str(self.height / 2))
                rect.set("height", str(self.options.bar_width))
                rect.set("width", str(normedvalue))

            rect.set("style", "fill:" + self.get_color())

            # If text is given, draw short paths and add the text
            # TODO: apply overlap workaround for visible gaps in between
            if keys:
                if not self.options.rotate:
                    x1 = (self.width + self.options.bar_width) / 2
                    y1 = y - offset - (normedvalue / 2)
                    x2 = self.options.bar_width / 2 + self.options.text_offset
                    y2 = 0
                    txt = self.width / 2 + self.options.bar_width + self.options.text_offset + 1
                    tyt = y - offset + self.fontoff - (normedvalue / 2)
                else:
                    x1 = x + offset + normedvalue / 2
                    y1 = y + self.options.bar_width / 2
                    x2 = 0
                    y2 = self.options.bar_width / 2 + (self.options.font_size \
                            * cnt) + self.options.text_offset
                    txt = x + offset + normedvalue / 2 - self.fontoff
                    tyt = (y) + self.options.bar_width + (self.options.font_size \
                            * (cnt + 1)) + self.options.text_offset

                elem = inkex.PathElement()
                elem.path = [Move(x1, y1), line(x2, y2)]
                elem.style = {
                    'fill': 'none',
                    'stroke': self.options.font_color,
                    'stroke-width': self.options.stroke_width,
                    'stroke-linecap': 'butt',
                }
                yield elem
                yield self.draw_text(keys[cnt], x=str(txt), y=str(tyt))

            # Increase Offset and Color
            offset = offset + normedvalue
            color = (color + 1) % 8

            # Draw rectangle
            yield rect

        yield self.draw_header(self.width / 2 + offset + normedvalue)


if __name__ == '__main__':
    NiceChart().run()
