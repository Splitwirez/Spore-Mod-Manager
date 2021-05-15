#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2012 Martin Owens
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 3.0 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
# Lesser General Public License for more details.
#
"""
This is a test framework setup.py only, not used for packaging.
"""

from setuptools import setup

setup(
        name='inkscape-core-extensions',
        version='0.0',
        description='Inkscape core extensions for testing',
        long_description='N/A',
        author='Inkscape Authors',
        url='https://gitlab.com/inkscape/extensions',
        author_email='developers@inkscape.org',
        test_suite='tests',
        platforms='linux',
        license='GPLv2',
        classifiers=[
            'Development Status :: 0 - Test Only',
            'Intended Audience :: Developers',
            'Programming Language :: Python',
            'Programming Language :: Python :: 2.7',
            'Programming Language :: Python :: 3.6',
            'Programming Language :: Python :: 3.7',
        ],
        install_requires=['scour', 'numpy', 'pyserial'],
        setup_requires=["pytest-runner"],
        tests_require=["pytest", "pytest-cov"]
)
