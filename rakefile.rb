begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


@solution = FubuRake::Solution.new do |sln|
	sln.compile = {
		:solutionfile => 'src/fubu.sln'
	}
				 
	sln.assembly_info = {
		:product_name => "fubu",
		:copyright => 'Copyright 2010-2013 Jeremy D. Miller, Josh Arnold, et al. All rights reserved.'
	}
	
	sln.ripple_enabled = true
	sln.fubudocs_enabled = true
	sln.ci_steps = [:archive_gem]
end


desc "Replaces the existing installed gem with the new version for local testing"
task :local_gem => [:create_gem] do
	sh 'gem uninstall fubu'
	Dir.chdir 'pkg' do
	    sh 'gem install fubu'
    end
end

desc "Moves the gem to the archive folder"
task :archive_gem => [:create_gem] do
	copyOutputFiles "pkg", "*.gem", "artifacts"
end

desc "Outputs the command line usage"
task :dump_usages => [:compile] do
  bottles 'dump-usages fubu src/Fubu.Docs/fubu.cli.xml'
end

desc "Creates the gem for fubu.exe"
task :create_gem => [:compile, :ilrepack] do
    require "rubygems/package"
	cleanDirectory 'bin';	
	cleanDirectory 'pkg'
	
	File.delete "src/fubu/bin/#{COMPILE_TARGET}/fubu.vshost.exe" unless !File.exist?("src/fubu/bin/#{COMPILE_TARGET}/fubu.vshost.exe")
	
	copyOutputFiles "src/fubu/bin/#{COMPILE_TARGET}", '*.dll', 'bin'
	copyOutputFiles "src/fubu/bin/#{COMPILE_TARGET}", '*.exe', 'bin'
	
	FileUtils.copy 'fubu', 'bin'


	spec = Gem::Specification.new do |s|
	  s.platform    = Gem::Platform::RUBY
	  s.name        = 'fubu'
	  s.version     = BUILD_NUMBER
	  s.files = Dir['bin/**/*']
	  s.bindir = 'bin'
	  s.executables << 'fubu'
	  
	  s.summary     = 'Command line tools for FubuMVC development'
	  s.description = 'Command line tools for FubuMVC development'
	  
	  s.authors           = ['Jeremy D. Miller', 'Josh Arnold', 'Chad Myers', 'Joshua Flanagan']
	  s.email             = 'fubumvc-devel@googlegroups.com'
	  s.homepage          = 'http://fubu-project.org'
	  s.rubyforge_project = 'fubu'
	end   
    puts "ON THE FLY SPEC FILES"
    puts spec.files
    puts "=========="

    Gem::Package::build spec, true
end

