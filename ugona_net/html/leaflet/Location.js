var Location = {

    update: function () {
        if (map == null)
            return;
        if ((this.init == null) && (window.navigator.geolocation != null)) {
            this.init = true;
            window.navigator.geolocation.watchPosition(function (position) {
                var c = position.coords;
                var data = c.latitude + ',' + c.longitude + ',' + c.accuracy;
                android.location = function () {
                    return data;
                }
                Location.data = android.location();
                Location.update();
            }, function (err) {
            })
        }
        if (this.data == null) {
            if (this.location) {
                map.removeLayer(this.location);
                map.removeLayer(this.marker);
                this.location = null;
                this.marker = null;
            }
            return;
        }
        var location = this.data.split(',');
        if (location.length < 3)
            return;
        var lat = location[0];
        var lng = location[1];
        var radius = parseFloat(location[2]);
        if (this.icon == null) {
            this.icon = L.icon({
                iconUrl: 'leaflet/images/person.png',
                iconSize: [48, 48],
                iconAnchor: [24, 48]
            });
        }
        if (this.location == null) {
            this.location = new L.Circle([lat, lng], radius, {
                stroke: true,
                color: '#0000FF',
                weight: 2,
                opacity: 0.1,
                fill: true,
                fillColor: '#0000FF',
                fillOpacity: 0.05
            });
            map.addLayer(this.location);
            this.marker = new L.Marker([lat, lng], {
                icon: this.icon
            });
            map.addLayer(this.marker);
        } else {
            this.location.setLatLng([lat, lng]);
            this.location.setRadius(radius);
            this.marker.setLatLng([lat, lng]);
            this.marker.setIcon(this.icon);
        }
    }
}

function myLocation() {

    Location.data = android.location();
    Location.update();
}

function setPosition() {
    if (map == null)
        return;

    var res = android.location();
    if (res == null)
        return;

    var pos = res.split(',');
    if (pos.length < 3)
        return;

    var bounds = getBounds();
    bounds.push([parseFloat(pos[0]), parseFloat(pos[1])]);

    fitBounds(bounds, 0.125);
}

function getRect(bounds) {
    var min_lat = 180;
    var min_lng = 180;
    var max_lat = -180;
    var max_lng = -180;
    for (var i = 0; i < bounds.length; i++) {
        var point = bounds[i];
        var lat = point[0];
        var lng = point[1];
        if (lat < min_lat)
            min_lat = lat;
        if (lat > max_lat)
            max_lat = lat;
        if (lng < min_lng)
            min_lng = lng;
        if (lng > max_lng)
            max_lng = lng;
    }
    return [
		[min_lat, min_lng],
		[max_lat, max_lng]
    ]
}

function fitBounds(bounds, k) {
    var rect = getRect(bounds);
    var min_point = rect[0];
    var max_point = rect[1];
    var min_lat = min_point[0];
    var min_lng = min_point[1];
    var max_lat = max_point[0];
    var max_lng = max_point[1];
    var d_lat = (max_lat - min_lat) * k;
    var d_lng = (max_lng - min_lng) * k;
    var zoom = map.getZoom();
    if (zoom < 12)
        zoom = 12;
    map.fitBounds([
		[min_lat - d_lat, min_lng - d_lng],
		[max_lat + d_lat, max_lng + d_lng]
    ], {
        maxZoom: zoom,
        animate: true
    });
}


function center() {
    if (map == null)
        return;
    fitBounds(getBounds(), 0.125);
    Points.showPopup();
}

var colors =
	[
		[5, '#800000'],
		[10, '#C00000'],
		[20, '#C04000'],
		[30, '#C08000'],
		[40, '#A08000'],
		[50, '#408000'],
		[60, '#00A000'],
		[90, '#00A020'],
		[0, '#00A080']
	];

var Tracks = {
    update: function () {
        for (var i in this.markers) {
            (function (p) {
                var lat = parseFloat(p[0]);
                var lon = parseFloat(p[1]);
                var mark = L.marker([lat, lon]);
                map.addLayer(mark);
                mark.on('click', function () {
                    showPopup(lat, lon, p[2])
                });
            })(this.markers[i]);
        }
        if (this.tracks) {
            for (var i in this.tracks) {
                map.removeLayer(this.tracks[i]);
            }
        }
        this.tracks = [];
        var traffic = android.speed();
        for (var i in this.points) {
            var p = this.points[i];
            var line = L.polyline(p.points, {
                color: traffic ? colors[p.index][1] : '#000080',
                weight: 7,
                opacity: 1
            })
				.addTo(map);
            this.tracks.push(line);
        }
        map.on('click', showPointInfo)
    },

    init: function () {
        var track_data = android.getTrack();
        this.parts = (track_data + "").split('|');
        this.initTracks();
    },


    initTracks: function () {
        this.points = [];
        this.markers = [];

        this.min_lat = 180;
        this.min_lng = 180;
        this.max_lat = -180;
        this.max_lng = -180;

        var last_mark = false;
        for (var i in this.parts) {
            var p = this.parts[i].split(',');
            var lat = parseFloat(p[0]);
            var lon = parseFloat(p[1]);
            if (lat > this.max_lat)
                this.max_lat = lat;
            if (lat < this.min_lat)
                this.min_lat = lat;
            if (lon > this.max_lng)
                this.max_lng = lon;
            if (lon < this.min_lng)
                this.min_lng = lon;
            if (p.length == 4) {
                var speed = parseFloat(p[2]);
                for (var index = 0; index < colors.length - 1; index++) {
                    if (colors[index][0] >= speed)
                        break;
                }
                var point = [lat, lon];
                if (last_mark) {
                    last_mark = false;
                } else {
                    if (this.points.length) {
                        var last = this.points[this.points.length - 1];
                        last.points.push(point);
                        if (last.index == index)
                            index = -1;
                    }
                }
                if (index >= 0) {
                    var last = {
                        index: index,
                        points: [point]
                    }
                    this.points.push(last);
                }
            } else if (p.length == 3) {
                this.markers.push(p);
                last_mark = true;
            } else {
                last_mark = true;
            }
        }
    },

}

function showPointInfo(event) {
    var best_p0 = null;
    var best_p = null;
    var best_dist = 1024;
    var best_pos = 0;
    var ep = map.latLngToLayerPoint(event.latlng);
    var p0 = null;
    for (var i in Tracks.parts) {
        var p = Tracks.parts[i].split(',');
        if (p.length != 4)
            continue;
        var lat = parseFloat(p[0]);
        var lon = parseFloat(p[1]);
        var point = map.latLngToLayerPoint(L.latLng(lat, lon));
        if (p0 == null) {
            p0 = p;
            continue;
        }
        var lat0 = parseFloat(p0[0]);
        var lon0 = parseFloat(p0[1]);
        var point0 = map.latLngToLayerPoint(L.latLng(lat0, lon0));
        if ((point0.x == point.x) && (point0.y == point.y)) {
            p0 = p;
            continue;
        }
        var cax = ep.x - point0.x;
        var cay = ep.y - point0.y;
        var bax = point.x - point0.x;
        var bay = point.y - point0.y;

        var pp = cax * bax + cay * bay;

        var dist;
        var pos;

        if (pp <= 0) {
            dist = cax * cax + cay * cay;
            pos = 0;
        } else {
            var l = bax * bax + bay * bay;
            if (pp >= l) {
                var cbx = ep.x - point.x;
                var cby = ep.y - point.y;
                dist = cbx * cbx + cby * cby;
                pos = 1000;
            } else {
                pos = 1000 * pp / l;
                bax = bax * pp / l;
                bay = bay * pp / l;
                cax -= bax;
                cay -= bay;
                dist = cax * cax + cay * cay;
            }
        }
        if (dist < best_dist) {
            best_p = p;
            best_p0 = p0;
            best_pos = pos;
            best_dist = dist;
        }
        p0 = p;
    }
    if (best_p == null)
        return;

    var lat0 = parseFloat(best_p0[0]);
    var lon0 = parseFloat(best_p0[1]);
    var speed0 = parseFloat(best_p0[2]);
    var time0 = parseFloat(best_p0[3]);

    var lat = parseFloat(best_p[0]);
    var lon = parseFloat(best_p[1]);
    var speed = parseFloat(best_p[2]);
    var time = parseFloat(best_p[3]);

    var lat = lat0 + (lat - lat0) * best_pos / 1000;
    var lon = lon0 + (lon - lon0) * best_pos / 1000;
    var speed = Math.ceil(speed0 + (speed - speed0) / 1000);
    var d = new Date(time0 + (time - time0) / 1000);
    showPopup(lat, lon, d.toLocaleTimeString() + '<br/>' + speed + ' ' + android.kmh());
}


function showPopup(lat, lon, text, point) {
    if (Tracks.point_info == null)
        Tracks.point_info = L.popup({
            maxWidth: 240
        });
    Tracks.popup = point;
    Tracks.point_info
		.setLatLng([lat, lon])
		.setContent(text)
		.addTo(map);
    Tracks.point_info.on('close', function () {
        Tracks.popup = null;
    })
}