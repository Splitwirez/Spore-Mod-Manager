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
#

import inkex
from inkex.transforms import DirectedLineSegment
from inkex.localization import inkex_gettext as _

class Envelope(inkex.EffectExtension):
    """Distort a path/group of paths to a second path"""
    def effect(self):
        if len(self.svg.selection) != 2:
            raise inkex.AbortExtension(_("You must select two objects only."))

        obj, envelope = self.svg.selection.values()

        if isinstance(obj, (inkex.PathElement, inkex.Group)):
            if isinstance(envelope, inkex.PathElement):
                # Get bounding box plus any extra composed transform of parents.
                bbox = obj.bounding_box(obj.getparent().composed_transform())

                # distill trafo into four node points
                path = envelope.path.transform(envelope.composed_transform()).to_superpath()
                if len(path[0]) < 4:
                    raise inkex.AbortExtension(_("Selected path is too short. Must be four or more nodes."))

                trafo = [[(csp[1][0], csp[1][1]) for csp in subs] for subs in path][0][:4]

                #vectors pointing away from the trafo origin
                tbox = [
                    DirectedLineSegment(trafo[0], trafo[1]),
                    DirectedLineSegment(trafo[1], trafo[2]),
                    DirectedLineSegment(trafo[3], trafo[2]),
                    DirectedLineSegment(trafo[0], trafo[3]),
                ]
            else:
                if isinstance(envelope, inkex.Group):
                    raise inkex.AbortExtension(_("The second selected object is a group, not a"
                                                 " path.\nTry using Object->Ungroup."))
                raise inkex.AbortExtension(_("The second selected object is not a path.\nTry using"
                                             " the procedure Path->Object to Path."))
        else:
            raise inkex.AbortExtension(_("The first selected object is neither a path nor a group.\nTry using"
                                         " the procedure Path->Object to Path."))

        self.process_object(obj, tbox, bbox)

    def process_object(self, obj, tbox, bbox):
        if isinstance(obj, inkex.PathElement):
            self.process_path(obj, tbox, bbox)
        elif isinstance(obj, inkex.Group):
            self.process_group(obj, tbox, bbox)

    def process_group(self, group, tbox, bbox):
        """Go through all groups to process all paths inside them"""
        for node in group:
            self.process_object(node, tbox, bbox)

    def process_path(self, element, tbox, bbox):
        # Get out path's absolute and root coordinates, so obj and envelope
        # are always in the same coordinate system.
        points = element.path.to_absolute().transform(element.composed_transform()).to_superpath()

        for subs in points:
            for csp in subs:
                csp[0] = self.transform_point(tbox, bbox, *csp[0])
                csp[1] = self.transform_point(tbox, bbox, *csp[1])
                csp[2] = self.transform_point(tbox, bbox, *csp[2])

        # Put the modified path back, undo the root transformation
        element.path = points.to_path().transform(-element.composed_transform())

    @staticmethod
    def transform_point(tbox, bbox, x, y):
        """Transform algorithm thanks to Jose Hevia (freon)"""
        vector = DirectedLineSegment((bbox.left, bbox.top), (x, y))
        xratio = vector.dx / bbox.width
        yratio = vector.dy / bbox.height
        horz = DirectedLineSegment(tbox[0].point_at_ratio(xratio), tbox[2].point_at_ratio(xratio))
        vert = DirectedLineSegment(tbox[3].point_at_ratio(yratio), tbox[1].point_at_ratio(yratio))
        return vert.intersect(horz)

if __name__ == '__main__':
    Envelope().run()
