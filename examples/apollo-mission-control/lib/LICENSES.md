# Third-Party Licenses

> **How these files are maintained.** The `*.min.js` bundles in this directory
> are vendored copies, refreshed from `node_modules/` via the repo's npm
> workflow. **Do not edit these files directly.** To update a vendored lib:
>
> 1. Bump the version constraint in repo-root `package.json`.
> 2. From repo root, run `npm install` (refreshes `node_modules/` and
>    `package-lock.json`).
> 3. Run `npm run vendor` (executes `scripts/vendor.js`, copies the new
>    `mqtt.min.js` / `topojson-client.min.js` into both consoles' `lib/`
>    directories).
> 4. If a major version changed, update the version line below and verify
>    the example consoles still load and connect against a running broker.
> 5. Commit `package.json`, `package-lock.json`, the new `*.min.js` files,
>    and this `LICENSES.md` together.
>
> `three.min.js` (r128) and `land-110m.json` are NOT covered by the npm
> workflow — they were vendored manually and are maintained by hand.

## Three.js (r128)
Source: https://github.com/mrdoob/three.js
File: three.min.js

The MIT License
Copyright (c) 2010-2021 three.js authors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

---

## MQTT.js (4.3.7)
Source: https://github.com/mqttjs/MQTT.js
File: mqtt.min.js

The MIT License (MIT)
Copyright (c) 2015-2016 MQTT.js contributors
Copyright 2011-2014 by Adam Rudd

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

---

## topojson-client (3.x)
Source: https://github.com/topojson/topojson-client
File: topojson-client.min.js

Copyright 2012-2019 Michael Bostock

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
PERFORMANCE OF THIS SOFTWARE.

---

## Natural Earth / world-atlas (2.x)
Source: https://github.com/topojson/world-atlas
Data: https://www.naturalearthdata.com
File: land-110m.json

Natural Earth data is in the public domain.
The world-atlas package is by Mike Bostock, ISC License (same as topojson-client above).
