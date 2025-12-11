@tool
extends EditorPlugin

var node:Node

func _enter_tree() -> void:
	var res := load("res://addons/LFFramework/Editor/json_edit/json_edit.tscn");
	node = res.instantiate() as Node;
	add_control_to_bottom_panel(node,"Json 编辑");
	
func _exit_tree() -> void:
	remove_control_from_bottom_panel(node);
	node.queue_free();
