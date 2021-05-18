#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005 Aaron Spike, aaron@ekips.org
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
"""
Perspective approach & math by Dmitry Platonov, shadowjack@mail.ru, 2006
"""

import inkex
from inkex.localization import inkex_gettext as _

X, Y = range(2)

try:
    import numpy as np
    import numpy.linalg as lin
    FLOAT = np.float64
except ImportError:
    np = None


class Perspective(inkex.EffectExtension):
    """Apply a perspective to a path/group of paths"""
    def effect(self):
        if np is None:
            raise inkex.AbortExtension(
                _("Failed to import the numpy or numpy.linalg modules."
                  " These modules are required by this extension. Please install them."
                  "  On a Debian-like system this can be done with the command, "
                  "sudo apt-get install python-numpy."))
        if len(self.svg.selection) != 2:
            raise inkex.AbortExtension(_("This extension requires two selected objects."))

        obj, envelope = self.svg.selection.values()

        if isinstance(obj, (inkex.PathElement, inkex.Group)):
            if isinstance(envelope, inkex.PathElement):
                path = envelope.path.transform(envelope.composed_transform()).to_superpath()

                if len(path) < 1 or len(path[0]) < 4:
                    raise inkex.AbortExtension(
                        _("This extension requires that the second path be four nodes long."))

                dip = np.zeros((4, 2), dtype=FLOAT)
                for i in range(4):
                    dip[i][0] = path[0][i][1][0]
                    dip[i][1] = path[0][i][1][1]

                # Get bounding box plus any extra composed transform of parents.
                bbox = obj.bounding_box(obj.getparent().composed_transform())

                sip = np.array([
                    [bbox.left, bbox.bottom],
                    [bbox.left, bbox.top],
                    [bbox.right, bbox.top],
                    [bbox.right, bbox.bottom]], dtype=FLOAT)
            else:
                if isinstance(envelope, inkex.Group):
                    raise inkex.AbortExtension(_("The second selected object is a group, not a"
                                                 " path.\nTry using Object->Ungroup."))
                raise inkex.AbortExtension(_("The second selected object is not a path.\nTry using"
                                             " the procedure Path->Object to Path."))
        else:
                raise inkex.AbortExtension(_("The first selected object is neither a path nor a group.\nTry using"
                                         " the procedure Path->Object to Path."))

        solmatrix = np.zeros((8, 8), dtype=FLOAT)
        free_term = np.zeros(8, dtype=FLOAT)
        for i in (0, 1, 2, 3):
            solmatrix[i][0] = sip[i][0]
            solmatrix[i][1] = sip[i][1]
            solmatrix[i][2] = 1
            solmatrix[i][6] = -dip[i][0] * sip[i][0]
            solmatrix[i][7] = -dip[i][0] * sip[i][1]
            solmatrix[i + 4][3] = sip[i][0]
            solmatrix[i + 4][4] = sip[i][1]
            solmatrix[i + 4][5] = 1
            solmatrix[i + 4][6] = -dip[i][1] * sip[i][0]
            solmatrix[i + 4][7] = -dip[i][1] * sip[i][1]
            free_term[i] = dip[i][0]
            free_term[i + 4] = dip[i][1]

        res = lin.solve(solmatrix, free_term)
        projmatrix = np.array([
            [res[0], res[1], res[2]],
            [res[3], res[4], res[5]],
            [res[6], res[7], 1.0]], dtype=FLOAT)

        self.process_object(obj, projmatrix)

    def process_object(self, obj, matrix):
        if isinstance(obj, inkex.PathElement):
            self.process_path(obj, matrix)
        elif isinstance(obj, inkex.Group):
            self.process_group(obj, matrix)

    def process_group(self, group, matrix):
        """Go through all groups to process all paths inside them"""
        for node in group:
            self.process_object(node, matrix)

    def process_path(self, element, matrix):
        """Apply the transformation to the selected path"""
        point = element.path.to_absolute().transform(element.composed_transform()).to_superpath()
        for subs in point:
            for csp in subs:
                csp[0] = self.project_point(csp[0], matrix)
                csp[1] = self.project_point(csp[1], matrix)
                csp[2] = self.project_point(csp[2], matrix)
        element.path = inkex.Path(point).transform(-element.composed_transform())

    @staticmethod
    def project_point(point, matrix):
        """Apply the matrix to the given point"""
        return [(point[X] * matrix[0][0] + point[Y] * matrix[0][1] + matrix[0][2]) /
                (point[X] * matrix[2][0] + point[Y] * matrix[2][1] + matrix[2][2]),
                (point[X] * matrix[1][0] + point[Y] * matrix[1][1] + matrix[1][2]) /
                (point[X] * matrix[2][0] + point[Y] * matrix[2][1] + matrix[2][2])]


if __name__ == '__main__':
    Perspective().run()
