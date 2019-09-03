#encoding=utf-8
require "find"
require "fileutils"

result = {}	# 认为对话只有一行
Find.find("../Assets/resources/prefabs/maps/") {|filename|
	p filename
	next if File.directory?(filename)
	File.open(filename, "rb"){|file|
		file.each_line{|line|
			line.force_encoding('utf-8')
			eval("\"#{$1}\"").split(//).each{|c|result[c] = true} if line[/"(.*)?"$/]
		}
	}
}
Find.find("../Assets/resources/data/") {|filename|
	p filename
	next if File.directory?(filename)
	File.open(filename, "rb"){|file|
		file.each_line{|line|
			line.force_encoding('utf-8')
			line.split(//).each{|c|result[c] = true}
		}
	}
}
Find.find("../Assets/resources/prefabs/ui/menus/") {|filename|
	p filename
	next if File.directory?(filename)
	File.open(filename, "rb"){|file|
		file.each_line{|line|
			line.force_encoding('utf-8')
			eval("\"#{$1}\"").split(//).each{|c|result[c] = true} if line[/"(.*)?"$/]
		}
	}
}
File.open("chars.txt", "w"){|file|
	file.puts result.keys.sort.join("")
}
