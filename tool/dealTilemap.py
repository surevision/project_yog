from PIL import Image as PIL_Image
import sys

if len(sys.argv) < 2:
	print("no file input.")
	sys.exit(0)

filename = sys.argv[1]

split = input('size:') # tile format

# open origin image
image = PIL_Image.open(filename)
w = image.size[0]
h = image.size[1]

nw = w + 1 * 2 * (w / split)
nh = h + 1 * 2 * (h / split)
newsize = (nw, nh)
newimage = PIL_Image.new("RGBA", newsize)

x = 0
y = 0

for i in range(0, (w / split) * (h / split)):
	col = i % (w / split)
	row = i / (w / split)
	x = col * split
	y = row * split
	print("x, y", x, y)
	# top
	top_edge = image.crop((x, y, x + split, y + 1))
	# bottom
	bottom_edge = image.crop((x, y + split - 1, x + split, y + split))
	# left
	left_edge = image.crop((x, y, x + 1, y + split))
	# right
	right_edge = image.crop((x + split - 1, y, x + split, y + split))

	targetX = (1 * 2 + split) * col + 1
	targetY = (1 * 2 + split) * row + 1
	
	newimage.paste(top_edge, (targetX, targetY - 1))
	newimage.paste(bottom_edge, (targetX, targetY + split))
	newimage.paste(left_edge, (targetX - 1, targetY))
	newimage.paste(right_edge, (targetX + split, targetY))

	# copy center
	center = image.crop((x, y, x + split, y + split))
	newimage.paste(center, (targetX, targetY))

# save new image
newimage.save("output_" + filename)