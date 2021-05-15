#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2012 Jabiertxo Arraiza, jabier.arraiza@marker.es
# Copyright (C) 2016 su_v, <suv-sf@users.sf.net>
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
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#
"""Document information"""

import inkex

class DocInfo(inkex.EffectExtension):
    """Show document information"""
    def effect(self):
        namedview = self.svg.namedview
        self.msg(":::SVG document related info:::")
        self.msg("version: " + self.svg.get('inkscape:version', 'New Document (unsaved)'))
        self.msg("width: {}".format(self.svg.width))
        self.msg("height: {}".format(self.svg.height))
        self.msg("viewbox: {}".format(str(self.svg.get_viewbox())))
        self.msg("document-units: {}".format(namedview.get('inkscape:document-units', 'None')))
        self.msg("units: " + namedview.get('units', 'None'))
        self.msg("Document has " + str(len(namedview.get_guides())) + " guides")
        for i, grid in enumerate(namedview.findall('inkscape:grid')):
            self.msg("Grid number {}: Units: {}".format(i + 1, grid.get("units", 'None')))

if __name__ == '__main__':
    DocInfo().run()
