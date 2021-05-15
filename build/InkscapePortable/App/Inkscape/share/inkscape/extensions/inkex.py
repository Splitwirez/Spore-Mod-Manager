# coding=utf-8
"""
This file only exists to not break extensions which have

    <dependency type="executable" location="extensions">inkex.py</dependency>

This module should never be imported, Python should automatically find
"inkex/__init__.py" before finding this file.
"""

raise Exception("FIXME: Python imported " + __file__)
