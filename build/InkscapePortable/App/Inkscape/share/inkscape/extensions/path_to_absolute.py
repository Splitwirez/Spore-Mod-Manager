#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2020 Martin Owens <doctormo@gmail.com>
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
Path To Absolute
"""
import inkex

class ToAbsolute(inkex.EffectExtension):
    """Convert any selected object to absolute/object-to-path/bezier only paths"""
    def effect(self):
        """Performs the effect."""
        for node in self.svg.selected.values():
            if not isinstance(node, inkex.PathElement):
                node = node.replace_with(node.to_path_element())
            node.path = node.path.to_absolute().to_superpath().to_path()

if __name__ == '__main__':
    ToAbsolute().run()
