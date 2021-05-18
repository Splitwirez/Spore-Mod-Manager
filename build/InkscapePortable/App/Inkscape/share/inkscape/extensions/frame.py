#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2016 Richard White, rwhite8282@gmail.com
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
"""
An Inkscape extension that creates a frame around a selected object.
"""

import inkex
from inkex import Group, PathElement, ClipPath

def size_box(box, delta):
    """ Returns a box with an altered size.
    delta -- The amount the box should grow.
    Returns a box with an altered size.
    """
    return (box.x.minimum - delta, box.x.maximum + delta,
            box.y.minimum - delta, box.y.maximum + delta)


# Frame maker Inkscape effect extension
class Frame(inkex.EffectExtension):
    """
    An Inkscape extension that creates a frame around a selected object.
    """
    def add_arguments(self, pars):
        # Parse the options.
        pars.add_argument('--tab', default='object')
        pars.add_argument('--clip', type=inkex.Boolean, default=False)
        pars.add_argument('--corner_radius', type=int, default=0)
        pars.add_argument('--fill_color', type=inkex.Color, default=inkex.Color(0))
        pars.add_argument('--group', type=inkex.Boolean, default=False)
        pars.add_argument('--position', default='outside')
        pars.add_argument('--stroke_color', type=inkex.Color, default=inkex.Color(0))
        pars.add_argument('--width', type=float, default=2.0)

    def add_clip(self, node, clip_path):
        """ Adds a new clip path node to the defs and sets
                the clip-path on the node.
            node -- The node that will be clipped.
            clip_path -- The clip path object.
        """
        clip = ClipPath()
        clip.append(PathElement(d=str(clip_path.path)))
        clip_id = self.svg.get_unique_id('clipPath')
        clip.set('id', clip_id)
        self.svg.defs.append(clip)
        node.set('clip-path', 'url(#{})'.format(str(clip_id)))

    def add_frame(self, name, box, style, radius=0):
        """
            name -- The name of the new frame object.
            box -- The boundary box of the node.
            style -- The style used to draw the path.
            radius -- The corner radius of the frame.
            returns a new frame node.
        """
        r = min([radius, (abs(box[1] - box[0]) / 2), (abs(box[3] - box[2]) / 2)])
        if radius > 0:
            d = ' '.join(str(x) for x in
                         ['M', box[0], (box[2] + r),
                          'A', r, r, '0 0 1', (box[0] + r), box[2],
                          'L', (box[1] - r), box[2],
                          'A', r, r, '0 0 1', box[1], (box[2] + r),
                          'L', box[1], (box[3] - r),
                          'A', r, r, '0 0 1', (box[1] - r), box[3],
                          'L', (box[0] + r), box[3],
                          'A', r, r, '0 0 1', box[0], (box[3] - r),
                          'Z'])
        else:
            d = ' '.join(str(x) for x in
                         ['M', box[0], box[2],
                          'L', box[1], box[2],
                          'L', box[1], box[3],
                          'L', box[0], box[3],
                          'Z'])

        elem = PathElement()
        elem.style = style
        elem.label = name
        elem.path = d
        return elem

    def effect(self):
        """Performs the effect."""
        # Determine common properties.
        width = self.options.width
        style = inkex.Style({'stroke-width': width})
        style.set_color(self.options.fill_color, 'fill')
        style.set_color(self.options.stroke_color, 'stroke')
        layer = self.svg.get_current_layer()

        for node in self.svg.selected.values():
            box = node.bounding_box()
            if self.options.position == 'outside':
                box = size_box(box, (width / 2))
            else:
                box = size_box(box, -(width / 2))

            frame = self.add_frame("Frame", box, style, self.options.corner_radius)
            if self.options.clip:
                self.add_clip(node, frame)
            if self.options.group:
                group = layer.add(Group())
                group.append(node)
                group.append(frame)
            else:
                layer.append(frame)
        return None

if __name__ == '__main__':
    Frame().run()
