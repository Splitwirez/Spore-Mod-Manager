# coding=utf-8
#
# Copyright (C) 2013 Public Domain
#               2018 Martin Owens <doctormo@gmail.com>
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

# standard libraries
from __future__ import unicode_literals

# local library
import inkex
from inkex.localization import inkex_gettext as _
from inkex.base import SvgOutputMixin

class hpglDecoder(SvgOutputMixin):
    def __init__(self, hpglString, options):
        """ options:
                "resolutionX":float
                "resolutionY":float
                "showMovements":bool
        """
        self.hpglString = hpglString
        self.options = options
        self.scaleX = options.resolutionX / 25.4  # dots/inch to dots/mm
        self.scaleY = options.resolutionY / 25.4  # dots/inch to dots/mm
        self.warning = ''
        self.textMovements = _("Movements")
        self.textPenNumber = _("Pen ")
        self.layers = {}
        self.oldCoordinates = (0.0, 297.0)

    def get_svg(self):
        """Generate an svg document from hgpl data"""
        actual_layer = 0
        # prepare document
        doc = self.get_template(width=210.0, height=297.0, unit='mm')
        svg = doc.getroot()
        svg.namedview.set('inkscape:document-units', 'mm')

        if self.options.showMovements:
            self.layers[0] = svg.add(inkex.Layer(self.textMovements))

        # cut stream into commands
        hpgl_data = self.hpglString.split(';')
        # if number of commands is under needed minimum, no data was found
        if len(hpgl_data) < 3:
            raise Exception('NO_HPGL_DATA')
        # decode commands into svg data
        for command in hpgl_data:
            if command.strip() != '':
                if command[:2] == 'IN' or command[:2] == 'FS' or command[:2] == 'VS':
                    # if Initialize, force or speed command ignore it
                    pass
                elif command[:2] == 'SP':
                    # if Select Pen command
                    actual_layer = int(command[2:])
                elif command[:2] == 'PU':
                    # if Pen Up command
                    self.parameters_to_path(svg, command[2:], 0, True)
                elif command[:2] == 'PD':
                    # if Pen Down command
                    self.parameters_to_path(svg, command[2:], actual_layer + 1, False)
                else:
                    self.warning = 'UNKNOWN_COMMANDS'
        return doc, self.warning

    def parameters_to_path(self, svg, parameters, layerNum, isPU):
        """split params and sanity check them"""
        parameters = parameters.strip().split(',')
        if parameters and len(parameters) % 2 == 0:
            for i, param in enumerate(parameters):
                # convert params to document units
                if i % 2 == 0:
                    parameters[i] = str(float(param) / self.scaleX)
                else:
                    parameters[i] = str(297.0 - (float(param) / self.scaleY))
            # create path and add it to the corresponding layer
            if not isPU or (self.options.showMovements and isPU):
                # create layer if it does not exist
                if layerNum not in self.layers:
                    label = self.textPenNumber + str(layerNum - 1)
                    self.layers[layerNum] = svg.add(inkex.Layer.new(label))

                path = 'M %f,%f L %s' % (self.oldCoordinates[0], self.oldCoordinates[1], ','.join(parameters))
                style = 'stroke:#' + ('ff0000' if isPU else '000000') + '; stroke-width:0.2; fill:none;'
                self.layers[layerNum].add(inkex.PathElement(d=path, style=style))
            self.oldCoordinates = (float(parameters[-2]), float(parameters[-1]))
