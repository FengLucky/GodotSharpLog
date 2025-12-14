@tool
extends EditorPlugin

const LogDebugger = preload("res://addons/GodotSharpLog/Editor/LogDebugger.gd")

var debugger: LogDebugger = null;

func _enter_tree() -> void:
	debugger = LogDebugger.new();
	add_debugger_plugin(debugger);	
	
func _exit_tree() -> void:
	if self.debugger != null:
		debugger.stop_all_profiler();
		remove_debugger_plugin(debugger);
		self.debugger = null;	
