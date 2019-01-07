# -- coding: utf-8 --
from PIL import Image as PIL_Image
import sys

if len(sys.argv) < 2:
	print("no file input.")
	sys.exit(0)
AUTOTILE_PARTS = [[18,17,14,13], [ 2,14,17,18], [13, 3,17,18], [ 2, 3,17,18],
				[13,14,17, 7], [ 2,14,17, 7], [13, 3,17, 7], [ 2, 3,17, 7],
				[13,14, 6,18], [ 2,14, 6,18], [13, 3, 6,18], [ 2, 3, 6,18],
				[13,14, 6, 7], [ 2,14, 6, 7], [13, 3, 6, 7], [ 2, 3, 6, 7],
				[16,17,12,13], [16, 3,12,13], [16,17,12, 7], [12, 3,16, 7], 
				[10, 9,14,13], [10, 9,14, 7], [10, 9, 6,13], [10, 9, 6, 7],
				[18,19,14,15], [18,19, 6,15], [ 2,19,14,15], [ 2,19, 6,15],
				[18,17,22,21], [ 2,17,22,21], [18, 3,22,21], [ 2, 3,21,22],
				[16,19,12,15], [10, 9,22,21], [ 8, 9,12,13], [ 8, 9,12, 7],
				[10,11,14,15], [10,11, 6,15], [18,19,22,23], [ 2,19,22,23],
				[16,17,20,21], [16, 3,20,21], [ 8,11,12,15], [ 8, 9,20,21],
				[16,19,20,23], [10,11,22,23], [ 8,11,20,23], [ 0, 1, 4, 5]]

filename = sys.argv[1]
print "convert %s" % (filename)

# open origin image
image = PIL_Image.open(filename)
w = image.size[0]
h = image.size[1]

def get_bitmap(id):
	bmp = PIL_Image.new("RGBA", (32, 32))
	sub_id = id - 2816
	autotile = sub_id / 48
	auto_id = sub_id % 48
	sx = (autotile % 8) * 64
	sy = (autotile / 8) * 96
	rects = AUTOTILE_PARTS[auto_id]
	for i in range(0,4):
		x = (rects[i] % 4) * 16 + sx
		y = (rects[i] / 4) * 16 + sy
		center = image.crop([x, y, x + 16, y + 16])
		# print "from %d %d to %d, %d" % (x, y, (i % 2) * 16, (i / 2) * 16)
		# print "size" 
		# print center.size
		bmp.paste(center, ((i % 2) * 16, (i / 2) * 16))
	print "%d finish" % (id)
	# bmp.save("temp/%d.png" % (id))
	return bmp

# 转换
fromId = 2816
toId = 4352
bitmaps = []
for i in range(fromId, toId):
	bitmaps.append(get_bitmap(i))
bmp = PIL_Image.new("RGBA", (32 * 8, 32 * 6))
for index in range(0, len(bitmaps)):
	if index % 48 == 0:
		bmp = PIL_Image.new("RGBA", (32 * 8, 32 * 6))
	bitmap = bitmaps[index]
	x = ((index % 48) % 8) * 32
	y = (index % 48) / 8 * 32
	center = bitmap.crop([0, 0, bitmap.size[0], bitmap.size[1]])
	bmp.paste(center, (x, y))
	if index % 48 == 47:
		bmp.save("A2/a2_%d.png" % (index / 48))

print "convert finish!"


