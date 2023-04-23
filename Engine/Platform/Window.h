#pragma once
#include "CommonHeaders.h"
namespace primal::platform {
	DEFINE_TYPED_ID(window_id);
	class window {
	public:
		constexpr explicit window(window_id id) : _id{ id } {}
		constexpr window() = default;
		constexpr window_id get_id() const { return _id; }
		constexpr bool is_valid() const { return id::Is_valid(_id); }
		void set_fullscreen(bool is_fullscreen) const;
		bool is_full_screen() const;
		void* handle() const;
		void set_caption(const wchar_t* caption) const;
		math::u32v4 size() const;
		void resize(u32 width, u32 height) const;
		u32 width() const;
		u32 height() const;
		bool is_closed() const;
	private:
		window_id _id{ id::invalid_id };
	};
}