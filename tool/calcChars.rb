#encoding=utf-8
require "find"
require "fileutils"

result = {}
Find.find("../Assets/resources/prefabs/maps/") {|filename|
	p filename
	next if File.directory?(filename)
	File.open(filename, "rb"){|file|
		file.each_line{|line|
			eval("\"#{$1}\"").split(//).each{|c|result[c] = true} if line[/"(.*)?"$/]
		}
	}
}
File.open("chars.txt", "w"){|file|
	file.puts result.keys.sort.join("")
}
